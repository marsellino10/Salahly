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
                try
                {
                    return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                    {
                        try
                        {
                            _logger.LogInformation($"Starting offer acceptance - Customer: {customerId}, Offer: {offerId}");

                            // ========== Step 1: Validate Offer ==========
                            var offer = await _offerService.GetOfferForAcceptanceAsync(offerId, customerId);
                            if (offer == null)
                            {
                                throw new InvalidOperationException("Offer not found");
                            }

                            // ========== Step 2: Load ServiceRequest ==========
                            var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(offer.ServiceRequestId);
                            if (serviceRequest == null)
                            {
                                throw new InvalidOperationException("Service request not found");
                            }

                            string paymentMethod = serviceRequest.PaymentMethod ?? "Card";

                            // ========== Step 3: Load Related Data ==========
                            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                            var craftsman = await _unitOfWork.Craftsmen.GetByIdAsync(offer.CraftsmanId);

                            if (customer == null || craftsman == null)
                            {
                                throw new InvalidOperationException("Required customer or craftsman data not found");
                            }

                            var customerUser = await _unitOfWork.ApplicationUsers.GetByIdAsync(customer.Id);
                            var craftsmanUser = await _unitOfWork.ApplicationUsers.GetByIdAsync(craftsman.Id);
                            var craft = await _unitOfWork.Crafts.GetByIdAsync(serviceRequest.CraftId);

                            if (customerUser == null || craftsmanUser == null || craft == null)
                            {
                                throw new InvalidOperationException("Required user or craft data not found");
                            }

                            // ========== Step 4: Create Booking & FIRST SAVE ==========
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

                            await _unitOfWork.SaveAsync(cancellationToken); // <--- FIRST SAVE

                            // ========== Step 5: Update Offer Statuses ==========
                            await _offerService.ReserveOfferAsync(offer.CraftsmanOfferId);
                            await _offerService.RejectOtherOffersAsync(
                                offer.ServiceRequestId, offerId, customerId);

                            // ========== Step 6: Create Payment Record ==========
                            var strategy = _paymentStrategyFactory.GetStrategy(paymentMethod);
                            var payment = await _paymentService.CreatePaymentRecordAsync(
                                booking.BookingId,
                                offer.OfferedPrice,
                                paymentMethod,
                                strategy.GetProviderName());

                            // ========== Step 7: Initialize Payment Gateway ==========
                            var paymentRequest = new PaymentInitializationRequest
                            {
                                BookingId = booking.BookingId,
                                PaymentId = payment.Id,
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
                                // Throwing exception here triggers Automatic Rollback for everything (Booking, Offers, Payment)
                                throw new InvalidOperationException(
                                    $"Payment initialization failed: {paymentResult.ErrorMessage}");
                            }

                            payment.TransactionId = paymentResult.TransactionId;
                            // Note: If you have properties like PaymentLink in your Payment entity, update them here too
                            // e.g., payment.PaymentLink = paymentResult.PaymentLink; 

                            // ========== Step 8: FINAL SAVE ==========
                            // Save changes to Payment (TransactionId) and Offers (Status)
                            await _unitOfWork.SaveAsync(cancellationToken);

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
                        }
                        catch (InvalidOperationException ex)
                        {
                            // Business logic errors (caught inside transaction to ensure rollback then rethrown)
                            throw;
                        }
                    }, cancellationToken);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Offer acceptance failed validation");
                    return WorkflowResult<BookingPaymentDto>.FailureResult(
                        ex.Message, "VALIDATION_ERROR");
                }
                catch (Exception ex)
                {
                    // System errors
                    _logger.LogError(ex, "Offer acceptance failed unexpectedly");
                    return WorkflowResult<BookingPaymentDto>.FailureResult(
                        "An unexpected error occurred processing the offer.", "SYSTEM_ERROR");
                }
            }
        }
    }