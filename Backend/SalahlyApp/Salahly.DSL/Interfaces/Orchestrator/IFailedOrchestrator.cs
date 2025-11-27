using Salahly.DSL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces.Orchestrator
{
    public interface IFailedOrchestrator
    {
        Task<WorkflowResult<bool>> ExecuteAsync(
            int bookingId,
            int paymentId,
            string failureReason,
            CancellationToken cancellationToken = default);
    }
}
