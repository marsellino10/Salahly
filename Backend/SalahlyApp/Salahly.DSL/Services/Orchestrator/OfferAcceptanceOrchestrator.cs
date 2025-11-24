using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.Booking;
using Salahly.DSL.DTOs.OffersDtos;
using Salahly.DSL.DTOs.PaymentDtos;
using Salahly.DSL.Interfaces;
using Salahly.DSL.Interfaces.Orchestrator;
using Salahly.DSL.Interfaces.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Services.Orchestrator
{
    /// <summary>
    /// Orchestrator for managing the complete offer acceptance workflow
    /// Handles transaction, rollback, and ensures ACID properties
    /// </summary>
    public class OfferAcceptanceOrchestrator : IAcceptOrchestrator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOfferService _offerService;
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentStrategyFactory _paymentStrategyFactory;
        private readonly ILogger<OfferAcceptanceOrchestrator> _logger;

        public OfferAcceptanceOrchestrator(
            IUnitOfWork unitOfWork,
            IOfferService offerService,
            IBookingService bookingService,
            IPaymentService paymentService,
            IPaymentStrategyFactory paymentStrategyFactory,
            ILogger<OfferAcceptanceOrchestrator> logger)
        {
            _unitOfWork = unitOfWork;
            _offerService = offerService;
            _bookingService = bookingService;
            _paymentService = paymentService;
            _paymentStrategyFactory = paymentStrategyFactory;
            _logger = logger;
        }

        public async Task<WorkflowResult<BookingPaymentDto>> ExecuteAsync(
    int customerId,
    int offerId,
    CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation(
                    $"Starting offer acceptance - Customer: {customerId}, Offer: {offerId}");

                // Step 1: Validate Offer
                var offer = await _offerService.GetOfferForAcceptanceAsync(offerId, customerId);
                if (offer == null)
                {
                    return WorkflowResult<BookingPaymentDto>.FailureResult(
                        "Offer not found", "ValidateOffer");
                }

                // Step 2: Load ServiceRequest and get PaymentMethod
                var serviceRequest = await _unitOfWork.ServiceRequests
                    .GetByIdAsync(offer.ServiceRequestId);

                if (serviceRequest == null)
                {
                    return WorkflowResult<BookingPaymentDto>.FailureResult(
                        "Service request not found", "LoadServiceRequest");
                }

                // PaymentMethod from ServiceRequest
                string paymentMethod = serviceRequest.PaymentMethod ?? "card";

                _logger.LogInformation($"Payment method: {paymentMethod}");

                // Step 3: Reserve Offer
                await _offerService.ReserveOfferAsync(offer);
                await _offerService.RejectOtherOffersAsync(
                    offer.ServiceRequestId, offerId, customerId);

                // Step 4: Load Related Data
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                var craftsman = await _unitOfWork.Craftsmen.GetByIdAsync(offer.CraftsmanId);
                var customerUser = await _unitOfWork.ApplicationUsers.GetByIdAsync(customer.Id);
                var craftsmanUser = await _unitOfWork.ApplicationUsers.GetByIdAsync(craftsman.Id);
                var craft = await _unitOfWork.Crafts.GetByIdAsync(serviceRequest.CraftId);

                if (customer == null || craftsman == null ||
                    customerUser == null || craftsmanUser == null || craft == null)
                {
                    return WorkflowResult<BookingPaymentDto>.FailureResult(
                        "Required data not found", "LoadRelatedData");
                }

                // Step 5: Create Booking
                var booking = await _bookingService.CreateBookingAsync(
                    customerId,
                    offer.CraftsmanId,
                    serviceRequest.CraftId,
                    offer.ServiceRequestId,
                    offerId,
                    offer.OfferedPrice,
                    serviceRequest.PreferredDate != default
                        ? serviceRequest.PreferredDate
                        : DateTime.UtcNow.AddDays(1));
                await _unitOfWork.SaveAsync(cancellationToken);

                // Step 6: Create Payment
                var strategy = _paymentStrategyFactory.GetStrategy(paymentMethod);
                var payment = await _paymentService.CreatePaymentRecordAsync(
                    booking.BookingId,
                    offer.OfferedPrice,
                    paymentMethod,
                    strategy.GetProviderName());

                // Step 7: Initialize Payment Gateway
                var paymentRequest = new PaymentInitializationRequest
                {
                    BookingId = booking.BookingId,
                    CustomerId = customerId,
                    Amount = offer.OfferedPrice,
                    CustomerEmail = customerUser.Email ?? "customer@test.com",
                    CustomerPhone = customer.PhoneNumber ?? customerUser.PhoneNumber ?? "01000000000",
                    CustomerName = customerUser.FullName ?? "Customer",
                    CustomerAddress = customer.Address ?? "Cairo",
                    CraftName = craft.Name ?? "Service",
                    CraftsmanName = craftsmanUser.FullName ?? "Craftsman",
                    BookingDate = booking.BookingDate,
                };

                var paymentResult = await _paymentService.InitializePaymentGatewayAsync(
                    payment, paymentRequest, cancellationToken);

                if (!paymentResult.IsSuccess)
                {
                    return WorkflowResult<BookingPaymentDto>.FailureResult(
                        $"Payment initialization failed: {paymentResult.ErrorMessage}",
                        "InitializePaymentGateway");
                }

                payment.TransactionId = paymentResult.TransactionId;

                _logger.LogInformation("Transaction committed successfully!");

                var response = new BookingPaymentDto
                {
                    BookingId = booking.BookingId,
                    PaymentId = payment.Id,
                    Amount = offer.OfferedPrice,
                    PaymentLink = paymentResult.PaymentLink,
                    PaymentToken = paymentResult.PaymentToken,
                    TransactionId = paymentResult.TransactionId,
                    BookingDate = booking.BookingDate,
                    PaymentDeadline = booking.PaymentDeadline,
                    CraftsmanName = craftsmanUser.FullName ?? "Craftsman",
                    CraftName = craft.Name ?? "Service"
                };

                return WorkflowResult<BookingPaymentDto>.SuccessResult(
                    response, "Offer accepted successfully");
            }, cancellationToken);
        }
    }
}
