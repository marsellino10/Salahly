using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.Booking;
using Salahly.DSL.Interfaces;
using SalahlyProject.Response;

namespace SalahlyProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ICraftsManService _craftsmanService;
        public AdminController(IBookingService bookingService, ICraftsManService craftsmanService)
        {
            _bookingService = bookingService;
            _craftsmanService = craftsmanService;
        }
        [HttpGet]
        public async Task<ActionResult<List<BookingAdminViewDto>>> GetAllStatics()
        {
            return await _bookingService.GetAllBookingsAsync();
        }
        [HttpGet("craftsmen")]
        public async Task<ActionResult<List<CraftsManAdminViewDto>>> GetAllCraftsmen()
        {
            return await _craftsmanService.GetAllAdmin();
        }
        //public async Task<ActionResult<List<CraftsManAdminViewDto>>> GetAllCraftsmen()
        //{
        //    return await _craftsmanService.GetAllWithFiltersAsync(new CraftsmanDto());
        //}
    }
}
