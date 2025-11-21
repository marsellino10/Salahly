using Salahly.DSL.DTOs.PaymentDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces.Payments
{
    public interface IPaymentStrategy
    {
        /// <summary>
        /// Initialize payment and return payment link/token
        /// </summary>
        Task<PaymentInitializationResult> InitializeAsync(
            PaymentInitializationRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify payment after callback
        /// </summary>
        Task<PaymentVerificationResult> VerifyAsync(
            string transactionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Process refund
        /// </summary>
        Task<RefundResult> RefundAsync(
            RefundRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get payment provider name
        /// </summary>
        string GetProviderName();
    }
}
