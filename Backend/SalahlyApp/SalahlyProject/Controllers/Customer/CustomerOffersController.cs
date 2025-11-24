using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs.OffersDtos;
using Salahly.DSL.Interfaces;
using Salahly.DSL.Interfaces.Orchestrator;
using System.Security.Claims;

namespace Salahly.API.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    [Route("api/customer")]
    [ApiController]
    public class CustomerOffersController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly IOfferAcceptanceOrchestrator _offerAcceptanceOrchestrator;
        private readonly ILogger<CustomerOffersController> _logger;

        public CustomerOffersController(IOfferService offerService, IOfferAcceptanceOrchestrator offerAcceptanceOrchestrator, ILogger<CustomerOffersController> logger)
        {
            _offerService = offerService;
            _offerAcceptanceOrchestrator = offerAcceptanceOrchestrator;
            _logger = logger;
        }

        private int GetCustomerIdFromToken()
        {
            var idClaim = User.FindFirst("NameIdentifier")?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        [HttpGet("service-requests/{requestId:int}/offers")]
        public async Task<IActionResult> GetOffersForMyRequest(int requestId)
        {
            var customerId = GetCustomerIdFromToken();
            if (customerId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid customer credentials" });

            var result = await _offerService.GetOffersForCustomerRequestAsync(customerId, requestId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("accept/{offerId}")]
        public async Task<IActionResult> AcceptOffer(int offerId)
        {
            try
            {
                // Get customer ID from JWT token
                var customerId = GetCustomerIdFromToken();

                var result = await _offerAcceptanceOrchestrator.ExecuteAsync(customerId, offerId);

                if (!result.Success)
                    return BadRequest(result);

                // Return payment info to frontend
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting offer");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPatch("offers/{offerId:int}/reject")]
        public async Task<IActionResult> RejectOffer(int offerId, [FromBody] RejectOfferDto dto)
        {
            var customerId = GetCustomerIdFromToken();
            if (customerId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid customer credentials" });

            var result = await _offerService.RejectOfferAsync(customerId, offerId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}