using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces.Orchestrator
{
    /// <summary>
    /// Orchestrator for handling the complete offer acceptance workflow
    /// Manages transaction and ensures data consistency
    /// </summary>
    public interface IAcceptOrchestrator
    {
        /// <summary>
        /// Execute complete workflow: Accept offer + Create booking + Initiate payment
        /// All within a single transaction - commits only after all steps succeed
        /// </summary>
        Task<WorkflowResult<BookingPaymentDto>> ExecuteAsync(
            int customerId,
            int offerId,
            CancellationToken cancellationToken = default);
    }
}
