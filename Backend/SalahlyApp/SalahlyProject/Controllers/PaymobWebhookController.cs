using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs.PaymentDtos;
using Salahly.DSL.Interfaces.Payments;

namespace SalahlyProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymobWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymobWebhookController> _logger;
        public PaymobWebhookController(IPaymentService paymentService, ILogger<PaymobWebhookController> logger )
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook(
            [FromBody] PaymobWebhookDto webhookData,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Webhook received for transaction: {webhookData.TransactionId}");

                var result = await _paymentService.ProcessWebhookAsync(
                    webhookData,
                    cancellationToken);

                if (result.Success)
                {
                    return Ok(new { message = "Webhook processed successfully" });
                }

                return BadRequest(new { error = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling webhook");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
