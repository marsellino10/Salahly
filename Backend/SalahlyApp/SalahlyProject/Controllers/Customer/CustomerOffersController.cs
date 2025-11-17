using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs.OffersDtos;
using Salahly.DSL.Interfaces;
using System.Security.Claims;

namespace Salahly.API.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    [Route("api/customer")]
    [ApiController]
    public class CustomerOffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public CustomerOffersController(IOfferService offerService)
        {
            _offerService = offerService;
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

        [HttpPatch("offers/{offerId:int}/accept")]
        public async Task<IActionResult> AcceptOffer(int offerId)
        {
            var customerId = GetCustomerIdFromToken();
            if (customerId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid customer credentials" });

            var result = await _offerService.AcceptOfferAsync(customerId, offerId);
            return result.Success ? Ok(result) : BadRequest(result);
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