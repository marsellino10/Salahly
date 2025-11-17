using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SalahlyProject.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using SalahlyProject.Response;
using Salahly.DSL.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SalahlyProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CraftsmanController : ControllerBase
    {
        private readonly ICraftsManService _service;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<CraftsmanController> _logger;

        public CraftsmanController(
            ICraftsManService service,
            IFileUploadService fileUploadService,
            ILogger<CraftsmanController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all craftsmen with optional filtering and pagination
        /// </summary>
        /// <param name="searchName">Search by craftsman full name (partial match)</param>
        /// <param name="craftId">Filter by craft ID</param>
        /// <param name="areaId">Filter by service area ID</param>
        /// <param name="isAvailable">Filter by availability status</param>
        /// <param name="minRating">Minimum rating average (0-5)</param>
        /// <param name="maxHourlyRate">Maximum hourly rate</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<CraftsmanDto>>>> GetAll([FromQuery] CraftsmanFilterDto craftmanFilter)
        {
            try
            {
                _logger.LogInformation("Getting all craftsmen with filters - SearchName: {SearchName}, CraftId: {CraftId}, Region: {Region}, City: {City}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                    craftmanFilter.SearchName, craftmanFilter.CraftId, craftmanFilter.Region, craftmanFilter.City, craftmanFilter.PageNumber, craftmanFilter.PageSize);


                // Get filtered and paginated results
                var result = await _service.GetAllWithFiltersAsync(craftmanFilter);

                return Ok(new ApiResponse<PaginatedResponse<CraftsmanDto>>(
                    200,
                    "Craftsmen retrieved successfully",
                    result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving craftsmen");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PaginatedResponse<CraftsmanDto>>(500, $"Error retrieving craftsmen: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get craftsman by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CraftsmanDto>>> Get(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<CraftsmanDto>(400, "Invalid craftsman ID"));

                _logger.LogInformation("Getting craftsman with ID: {CraftsmanId}", id);
                var item = await _service.GetByIdAsync(id);
                if (item == null)
                    return NotFound(new ApiResponse<CraftsmanDto>(404, $"Craftsman with ID {id} not found"));

                return Ok(new ApiResponse<CraftsmanDto>(200, "Craftsman retrieved successfully", item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving craftsman with ID: {CraftsmanId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CraftsmanDto>(500, $"Error retrieving craftsman: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new craftsman with optional profile image and service areas
        /// </summary>
        [Authorize(Roles ="Craftsman")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CraftsmanDto>>> Create([FromForm] CreateCraftsmanDto dto, IFormFile? profileImage, [FromForm] string? serviceAreasJson)
        {
            try
            {
                _logger.LogInformation("Creating craftsman with UserId: {UserId}, CraftId: {CraftId}", dto.UserId, dto.CraftId);

                // Deserialize serviceAreasJson if provided
                if (!string.IsNullOrWhiteSpace(serviceAreasJson))
                {
                    try
                    {
                        var serviceAreas = JsonSerializer.Deserialize<List<AddServiceAreaDto>>(serviceAreasJson);
                        if (serviceAreas != null && serviceAreas.Count > 0)
                        {
                            dto.ServiceAreas = serviceAreas;
                            _logger.LogInformation("Service areas deserialized: {ServiceAreaCount}", serviceAreas.Count);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Invalid JSON format for serviceAreasJson");
                        return BadRequest(new ApiResponse<CraftsmanDto>(400, $"Invalid JSON format for serviceAreasJson: {ex.Message}"));
                    }
                }
                dto.UserId = int.Parse(User.FindFirstValue("NameIdentifier"));
                if (dto.UserId <= 0) {
                    return BadRequest(new ApiResponse<CraftsmanDto>(400, "Invalid UserId in token"));
                }
                // Create the craftsman first
                var created = await _service.CreateAsync(dto);

                // Handle profile image upload if provided
                if (profileImage != null)
                {
                    try
                    {
                        var profileImageUrl = await _fileUploadService.UploadFileAsync(profileImage, "craftsmen");
                        
                        // Update craftsman with profile image URL
                        created = await _service.UpdateCraftsManImageAsync(created.Id, profileImageUrl);
                        _logger.LogInformation("Profile image uploaded successfully for craftsman ID: {CraftsmanId}", created.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error uploading profile image for craftsman ID: {CraftsmanId}. Continuing without image.", created.Id);
                        // Continue without profile image rather than failing the entire request
                    }
                }

                return CreatedAtAction(nameof(Get), new { id = created.Id }, 
                    new ApiResponse<CraftsmanDto>(201, "Craftsman created successfully", created));
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid input for craftsman creation");
                return BadRequest(new ApiResponse<CraftsmanDto>(400, $"Invalid input: {ex.Message}"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during craftsman creation");
                return BadRequest(new ApiResponse<CraftsmanDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for craftsman creation");
                return NotFound(new ApiResponse<CraftsmanDto>(404, $"User not found: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating craftsman");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CraftsmanDto>(500, $"Error creating craftsman: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an existing craftsman with optional profile image and service areas
        /// </summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CraftsmanDto>>> Update(int id, [FromForm] UpdateCraftsmanDto dto, IFormFile? profileImage, [FromForm] string? serviceAreasJson)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<CraftsmanDto>(400, "Invalid craftsman ID"));

                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<CraftsmanDto>(400, "Invalid model state", null));

                if (id != dto.Id)
                    return BadRequest(new ApiResponse<CraftsmanDto>(400, "ID in URL does not match ID in request body"));

                _logger.LogInformation("Updating craftsman with ID: {CraftsmanId}", id);

                // Deserialize serviceAreasJson if provided
                if (!string.IsNullOrWhiteSpace(serviceAreasJson))
                {
                    try
                    {
                        var serviceAreas = JsonSerializer.Deserialize<List<AddServiceAreaDto>>(serviceAreasJson);
                        if (serviceAreas != null && serviceAreas.Count > 0)
                        {
                            dto.ServiceAreas = serviceAreas;
                            _logger.LogInformation("Service areas deserialized: {ServiceAreaCount}", serviceAreas.Count);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Invalid JSON format for serviceAreasJson");
                        return BadRequest(new ApiResponse<CraftsmanDto>(400, $"Invalid JSON format for serviceAreasJson: {ex.Message}"));
                    }
                }

                // Handle profile image replacement if provided
                if (profileImage != null)
                {
                    try
                    {
                        // Get existing craftsman to retrieve old profile image URL
                        var existingCraftsman = await _service.GetByIdAsync(id);
                        if (existingCraftsman == null)
                            return NotFound(new ApiResponse<CraftsmanDto>(404, $"Craftsman with ID {id} not found"));

                        // Delete old profile image if it exists
                        if (!string.IsNullOrWhiteSpace(existingCraftsman.ProfileImageUrl))
                        {
                            try
                            {
                                await _fileUploadService.DeleteFileAsync(existingCraftsman.ProfileImageUrl);
                                _logger.LogInformation("Old profile image deleted for craftsman ID: {CraftsmanId}", id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete old profile image for craftsman ID: {CraftsmanId}", id);
                                // Continue without deleting old image
                            }
                        }

                        // Upload new profile image
                        var profileImageUrl = await _fileUploadService.UploadFileAsync(profileImage, "craftsmen");
                        await _service.UpdateCraftsManImageAsync(id, profileImageUrl);
                        _logger.LogInformation("New profile image uploaded for craftsman ID: {CraftsmanId}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error uploading profile image for craftsman ID: {CraftsmanId}. Continuing without new image.", id);
                        // Continue without new profile image rather than failing the entire request
                    }
                }

                // Update craftsman properties
                var updated = await _service.UpdateAsync(dto);
                return Ok(new ApiResponse<CraftsmanDto>(200, "Craftsman updated successfully", updated));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craftsman not found for update: {CraftsmanId}", id);
                return NotFound(new ApiResponse<CraftsmanDto>(404, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during craftsman update");
                return BadRequest(new ApiResponse<CraftsmanDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating craftsman with ID: {CraftsmanId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CraftsmanDto>(500, $"Error updating craftsman: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a craftsman and all associated files
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<object>(400, "Invalid craftsman ID"));

                _logger.LogInformation("Deleting craftsman with ID: {CraftsmanId}", id);

                // Get existing craftsman to retrieve profile and portfolio images
                var existingCraftsman = await _service.GetByIdAsync(id);
                if (existingCraftsman == null)
                    return NotFound(new ApiResponse<object>(404, $"Craftsman with ID {id} not found"));

                // Delete profile image
                if (!string.IsNullOrWhiteSpace(existingCraftsman.ProfileImageUrl))
                {
                    try
                    {
                        await _fileUploadService.DeleteFileAsync(existingCraftsman.ProfileImageUrl);
                        _logger.LogInformation("Profile image deleted for craftsman ID: {CraftsmanId}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete profile image for craftsman ID: {CraftsmanId}", id);
                        // Continue without failing
                    }
                }

                // Delete portfolio images
                if (existingCraftsman.Portfolio != null && existingCraftsman.Portfolio.Any())
                {
                    foreach (var portfolioItem in existingCraftsman.Portfolio)
                    {
                        if (!string.IsNullOrWhiteSpace(portfolioItem.ImageUrl))
                        {
                            try
                            {
                                await _fileUploadService.DeleteFileAsync(portfolioItem.ImageUrl);
                                _logger.LogInformation("Portfolio image deleted: {ImageUrl}", portfolioItem.ImageUrl);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete portfolio image: {ImageUrl}", portfolioItem.ImageUrl);
                                // Continue without failing
                            }
                        }
                    }
                }

                // Delete craftsman from database
                await _service.DeleteAsync(id);

                return Ok(new ApiResponse<object>(200, $"Craftsman with ID {id} successfully deleted", new { id }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craftsman not found for deletion: {CraftsmanId}", id);
                return NotFound(new ApiResponse<object>(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting craftsman with ID: {CraftsmanId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>(500, $"Error deleting craftsman: {ex.Message}"));
            }
        }
    }
}
