using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs
{
    /// <summary>
    /// Result wrapper for orchestrator workflows
    /// </summary>
    public class WorkflowResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FailedStep { get; set; }

        public static WorkflowResult<T> SuccessResult(T data, string? message = null)
        {
            return new WorkflowResult<T>
            {
                Success = true,
                Data = data,
                ErrorMessage = message
            };
        }

        public static WorkflowResult<T> FailureResult(string error, string? failedStep = null)
        {
            return new WorkflowResult<T>
            {
                Success = false,
                ErrorMessage = error,
                FailedStep = failedStep
            };
        }
    }
}
