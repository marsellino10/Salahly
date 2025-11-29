using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.Booking;
using Salahly.DSL.DTOs.PortfolioDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
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
        private readonly IAdminService _adminService;
        public AdminController(IBookingService bookingService, ICraftsManService craftsmanService, IAdminService adminService)
        {
            _bookingService = bookingService;
            _craftsmanService = craftsmanService;
            _adminService = adminService;
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

        [HttpGet("stats/service-requests/count")]
        public async Task<ActionResult<ApiResponse<int>>> CountServiceRequests([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int? craftId = null, [FromQuery] int? areaId = null)
        {
            try
            {
                var count = await _adminService.CountServiceRequestsAsync(from, to, craftId, areaId);
                return Ok(new ApiResponse<int>(200, "Count retrieved", count));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<int>(500, ex.Message));
            }
        }

        [HttpGet("stats/service-requests")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ServiceRequestDto>>>> GetServiceRequests([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int? craftId = null, [FromQuery] int? areaId = null, [FromQuery] string orderBy = "date", [FromQuery] bool asc = false)
        {
            try
            {
                var list = await _adminService.GetServiceRequestsFilteredAsync(from, to, craftId, areaId, orderBy, asc);
                return Ok(new ApiResponse<IEnumerable<ServiceRequestDto>>(200, "Service requests retrieved", list));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<IEnumerable<ServiceRequestDto>>(500, ex.Message));
            }
        }

        [HttpGet("stats/service-requests/most-active-area")]
        public async Task<ActionResult<ApiResponse<object>>> GetMostActiveArea()
        {
            try
            {
                var area = await _adminService.GetMostActiveAreaAsync();
                return Ok(new ApiResponse<object>(200, "Most active area retrieved", area));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(500, ex.Message));
            }
        }

        [HttpGet("stats/offers")]
        public async Task<ActionResult<ApiResponse<OffersStatsDto>>> GetOffersStats()
        {
            try
            {
                var s = await _adminService.GetOffersStatsAsync();
                return Ok(new ApiResponse<OffersStatsDto>(200, "Offers stats retrieved", s));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<OffersStatsDto>(500, ex.Message));
            }
        }

        [HttpGet("stats/craftsmen/count")]
        public async Task<ActionResult<ApiResponse<int>>> CountCraftsmen([FromQuery] int? craftId = null, [FromQuery] int? areaId = null)
        {
            try
            {
                var c = await _adminService.CountCraftsmenAsync(craftId, areaId);
                return Ok(new ApiResponse<int>(200, "Craftsmen count retrieved", c));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<int>(500, ex.Message));
            }
        }

        [HttpGet("stats/craftsmen/experience")]
        public async Task<ActionResult<ApiResponse<int>>> TotalCraftsmenExperience([FromQuery] int? craftId = null, [FromQuery] int? areaId = null)
        {
            try
            {
                var sum = await _adminService.GetTotalCraftsmenExperienceAsync(craftId, areaId);
                return Ok(new ApiResponse<int>(200, "Total experience retrieved", sum));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<int>(500, ex.Message));
            }
        }

        [HttpGet("stats/craftsmen/top")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CraftsmanShortDto>>>> GetTopCraftsmen([FromQuery] int top = 5)
        {
            try
            {
                var list = await _adminService.GetTopCraftsmenByReviewsAsync(top);
                return Ok(new ApiResponse<IEnumerable<CraftsmanShortDto>>(200, "Top craftsmen retrieved", list));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<IEnumerable<CraftsmanDto>>(500, ex.Message));
            }
        }

        [HttpGet("stats/crafts/count")]
        public async Task<ActionResult<ApiResponse<int>>> CountCrafts()
        {
            try
            {
                var c = await _adminService.CountCraftsAsync();
                return Ok(new ApiResponse<int>(200, "Crafts count retrieved", c));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<int>(500, ex.Message));
            }
        }

        [HttpGet("stats/crafts/average-reviews")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetCraftsAverageReviews()
        {
            try
            {
                var list = await _adminService.GetCraftsAverageReviewsAsync();
                return Ok(new ApiResponse<IEnumerable<object>>(200, "Crafts average reviews retrieved", list));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<IEnumerable<object>>(500, ex.Message));
            }
        }
        [HttpGet("portfolio/inactive")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioItemResponseDto>>>> GetInactivePortfolioItems()
        {
            try
            {
                var list = await _adminService.GetInactivePortfolioItemsAsync();
                return Ok(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(200, "Inactive portfolio items retrieved", list));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(500, ex.Message));
            }
        }

        [HttpPost("portfolio/{id}/approve")]
        public async Task<ActionResult<ApiResponse<object>>> ApprovePortfolioItem(int id)
        {
            try
            {
                var ok = await _adminService.ApprovePortfolioItemAsync(id);
                if (!ok) return NotFound(new ApiResponse<object>(404, "Portfolio item not found"));
                return Ok(new ApiResponse<object>(200, "Portfolio item approved", new { id }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(500, ex.Message));
            }
        }

    }
}
