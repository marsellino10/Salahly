// CraftsmanServiceRequestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using System.Security.Claims;

namespace Salahly.API.Controllers
{
    [Route("api/craftsman/service-requests")]
    [ApiController]
    [Authorize(Roles = "Craftsman")]
    public class CraftsmanServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;

        public CraftsmanServiceRequestController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }


        [HttpGet("opportunities")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<ServiceRequestDto>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAvailableOpportunities()
        {
            var craftsmanId = GetCraftsmanIdFromToken();

            if (craftsmanId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid craftsman credentials" });

            var result = await _serviceRequestService.GetAvailableOpportunitiesAsync(craftsmanId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("offers")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<ServiceRequestDto>>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetRequestsCraftsmanOfferedOn()
        {
            var craftsmanId = GetCraftsmanIdFromToken();

            if (craftsmanId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid craftsman credentials" });

            var result = await _serviceRequestService.GetRequestsWithCraftsmanOffersAsync(craftsmanId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{requestId:int}")]
        [ProducesResponseType(typeof(ServiceResponse<ServiceRequestDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetServiceRequestById(int requestId)
        {
            var craftsmanId = GetCraftsmanIdFromToken();

            if (craftsmanId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid craftsman credentials" });

            var result = await _serviceRequestService.GetServiceRequestForCraftsmanAsync(craftsmanId, requestId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
        private int GetCraftsmanIdFromToken()
        {
            var craftsmanIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(craftsmanIdClaim, out var id) ? id : 0;
        }
    }
}