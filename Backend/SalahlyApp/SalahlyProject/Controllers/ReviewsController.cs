using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using SalahlyProject.Response;
namespace SalahlyProject.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                //createReviewDto.ReviewerUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                // Automatic validation from DataAnnotations
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResponse<bool>(400, $"Validation failed: {string.Join(", ", errors)}", false));
                }

                // Additional business logic validation
                if (createReviewDto.ReviewerUserId == createReviewDto.TargetUserId)
                {
                    return BadRequest(new ApiResponse<bool>(400, "User cannot review themselves", false));
                }

                var result = await _reviewService.CreateReviewAsync(createReviewDto);

                if (!result)
                {
                    return Conflict(new ApiResponse<bool>(409, "User has already reviewed this booking or validation failed", false));
                }

                return Ok(new ApiResponse<bool>(200, "Review created successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for booking {BookingId}", createReviewDto?.BookingId);
                return StatusCode(500, new ApiResponse<bool>(500, "An error occurred while creating the review", false));
            }
        }

        [HttpDelete("{reviewId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReview(
            [FromRoute] int reviewId,
            [FromQuery, Required] int requestingUserId)
        {
            try
            {
                if (reviewId <= 0 || requestingUserId <= 0)
                {
                    return BadRequest(new ApiResponse<bool>(400, "Review ID and User ID must be greater than 0", false));
                }

                var result = await _reviewService.DeleteReviewAsync(reviewId, requestingUserId);

                if (!result)
                {
                    return NotFound(new ApiResponse<bool>(404, "Review not found or you don't have permission to delete it", false));
                }

                return Ok(new ApiResponse<bool>(200, "Review deleted successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId} for user {UserId}", reviewId, requestingUserId);
                return StatusCode(500, new ApiResponse<bool>(500, "An error occurred while deleting the review", false));
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CreateReviewDto>>>> GetReviewsForUser([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new ApiResponse<IEnumerable<CreateReviewDto>>(400, "User ID must be greater than 0"));
                }

                var reviews = await _reviewService.GetReviewsForUserAsync(userId);

                if (!reviews.Any())
                {
                    return NotFound(new ApiResponse<IEnumerable<CreateReviewDto>>(404, "No reviews found for this user"));
                }

                return Ok(new ApiResponse<IEnumerable<CreateReviewDto>>(200, "Reviews retrieved successfully", reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<IEnumerable<CreateReviewDto>>(500, "An error occurred while retrieving reviews"));
            }
        }

        [HttpGet("booking/{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CreateReviewDto>>>> GetReviewsForBooking([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return BadRequest(new ApiResponse<IEnumerable<CreateReviewDto>>(400, "Booking ID must be greater than 0"));
                }

                var reviews = await _reviewService.GetReviewsForBookingAsync(bookingId);

                if (!reviews.Any())
                {
                    return NotFound(new ApiResponse<IEnumerable<CreateReviewDto>>(404, "No reviews found for this booking"));
                }

                return Ok(new ApiResponse<IEnumerable<CreateReviewDto>>(200, "Reviews retrieved successfully", reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for booking {BookingId}", bookingId);
                return StatusCode(500, new ApiResponse<IEnumerable<CreateReviewDto>>(500, "An error occurred while retrieving reviews"));
            }
        }

        [HttpGet("user/{userId}/average-rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<double>>> GetAverageRating([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new ApiResponse<double>(400, "User ID must be greater than 0"));
                }

                var averageRating = await _reviewService.GetAverageRatingForUser(userId);

                return Ok(new ApiResponse<double>(200, "Average rating retrieved successfully", averageRating));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average rating for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<double>(500, "An error occurred while retrieving average rating"));
            }
        }
    }
}
