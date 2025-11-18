using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs.OffersDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using System.Security.Claims;

namespace Salahly.API.Controllers.Craftsman
{
    [Authorize(Roles = "Craftsman")]
    [Route("api/craftsman/offers")]
    [ApiController]
    public class CraftsmanOffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public CraftsmanOffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        private int GetCraftsmanIdFromToken()
        {
            var idClaim = User.FindFirst("NameIdentifier")?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOffer([FromBody] CreateOfferDto dto)
        {
            var craftsmanId = GetCraftsmanIdFromToken();
            if (craftsmanId == 0)
                return Unauthorized(new { Success = false, Message = "kosomk craftsman credentials." });

            var result = await _offerService.CreateOfferAsync(craftsmanId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOffers()
        {
            var craftsmanId = GetCraftsmanIdFromToken();
            if (craftsmanId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid craftsman credentials." });

            var result = await _offerService.GetOffersByCraftsmanAsync(craftsmanId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{offerId:int}")]
        public async Task<IActionResult> GetOfferById(int offerId)
        {
            var craftsmanId = GetCraftsmanIdFromToken();
            var result = await _offerService.GetOfferByIdForCraftsmanAsync(craftsmanId, offerId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPatch("{offerId:int}/withdraw")]
        public async Task<IActionResult> WithdrawOffer(int offerId)
        {
            var craftsmanId = GetCraftsmanIdFromToken();
            var result = await _offerService.WithdrawOfferAsync(craftsmanId, offerId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}