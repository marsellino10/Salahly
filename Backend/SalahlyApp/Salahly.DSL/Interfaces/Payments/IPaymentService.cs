using Salahly.DAL.Entities;
using Salahly.DSL.DTOs.PaymentDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces.Payments
{
    public interface IPaymentService
    {
        /// <summary>
        /// Verify payment after webhook callback
        /// </summary>
        Task<ServiceResponse<PaymentVerificationResult>> VerifyPaymentAsync(
            string transactionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Process Paymob webhook callback
        /// </summary>
        Task<ServiceResponse<bool>> ProcessWebhookAsync(
            PaymobWebhookDto webhookData,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete payment record
        /// <summary>
        Task DeletePaymentAsync(int paymentId);

        /// <summary>
        /// Create payment record
        /// <summary>
        Task<Payment> CreatePaymentRecordAsync(
            int bookingId,
            decimal amount,
            string paymentMethod,
            string gateway);

        /// <summary>
        /// initialize payment gateway
        /// <summary>
        Task<PaymentInitializationResult> InitializePaymentGatewayAsync(
            Payment payment,
            PaymentInitializationRequest request,
            CancellationToken cancellationToken = default);
    }
}
