using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.Interfaces;
using SalahlyProject.Response;
using System.Security.Claims;

namespace SalahlyProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private int GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("NameIdentifier")?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized(new ApiResponse<string>(401, "Invalid user"));

            var res = await _bookingService.GetBookingWithServiceRequestDetailsAsync(id, userId);
            if (!res.Success) return NotFound(new ApiResponse<string>(404, res.Message));

            return Ok(new ApiResponse<object>(200, "Booking retrieved", res.Data));
        }

        [HttpGet("")]
        public async Task<IActionResult> GetUserBookings()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized(new ApiResponse<string>(401, "Invalid user"));

            var res = await _bookingService.GetBookingsAsync(userId);
            if (!res.Success) return BadRequest(new ApiResponse<string>(400, res.Message));

            return Ok(new ApiResponse<object>(200, "Customer bookings retrieved", res.Data));
        }


        [HttpPost("{id:int}/confirm")]
        public async Task<IActionResult> Confirm(int id, CancellationToken cancellationToken)
        {
            var res = await _bookingService.ConfirmBookingAsync(id, cancellationToken);
            if (!res.Success) return BadRequest(new ApiResponse<string>(400, res.Message));
            return Ok(new ApiResponse<bool>(200, "Booking confirmed", res.Data));
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancellationRequest request, CancellationToken cancellationToken)
        {
            var res = await _bookingService.CancelBookingAsync(id, request?.Reason, cancellationToken);
            if (!res.Success) return BadRequest(new ApiResponse<string>(400, res.Message));
            return Ok(new ApiResponse<object>(200, "Booking cancelled", res.Data));
        }

        public class CancellationRequest { public string? Reason { get; set; } }
    }
}
