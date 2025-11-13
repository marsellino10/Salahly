using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Salahly.DSL.DTOs.PortfolioDtos;
using Salahly.DSL.Interfaces;
using SalahlyProject.Services.Interfaces;
using SalahlyProject.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalahlyProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioItemsController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<PortfolioItemsController> _logger;

        public PortfolioItemsController(
            IPortfolioService portfolioService,
            IFileUploadService fileUploadService,
            ILogger<PortfolioItemsController> logger)
        {
            _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all portfolio items for a specific craftsman
        /// </summary>
        [HttpGet("craftsman/{craftsmanId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioItemResponseDto>>>> GetByCraftsman(int craftsmanId)
        {
            try
            {
                if (craftsmanId <= 0)
                    return BadRequest(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(400, "Invalid craftsman ID"));

                _logger.LogInformation("Getting portfolio items for craftsman {CraftsmanId}", craftsmanId);
                var items = await _portfolioService.GetByCraftsmanAsync(craftsmanId);
                return Ok(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(200, "Portfolio items retrieved successfully", items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio items for craftsman {CraftsmanId}", craftsmanId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(500, $"Error retrieving portfolio items: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get active portfolio items for a specific craftsman (only active items)
        /// </summary>
        [HttpGet("craftsman/{craftsmanId}/active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioItemResponseDto>>>> GetActiveByCraftsman(int craftsmanId)
        {
            try
            {
                if (craftsmanId <= 0)
                    return BadRequest(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(400, "Invalid craftsman ID"));

                _logger.LogInformation("Getting active portfolio items for craftsman {CraftsmanId}", craftsmanId);
                var items = await _portfolioService.GetActiveByCraftsmanAsync(craftsmanId);
                return Ok(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(200, "Active portfolio items retrieved successfully", items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active portfolio items for craftsman {CraftsmanId}", craftsmanId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(500, $"Error retrieving active portfolio items: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific portfolio item by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PortfolioItemResponseDto>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, "Invalid portfolio item ID"));

                _logger.LogInformation("Getting portfolio item with ID: {PortfolioItemId}", id);
                var item = await _portfolioService.GetByIdAsync(id);

                if (item == null)
                    return NotFound(new ApiResponse<PortfolioItemResponseDto>(404, $"Portfolio item with ID {id} not found"));

                return Ok(new ApiResponse<PortfolioItemResponseDto>(200, "Portfolio item retrieved successfully", item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio item {PortfolioItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PortfolioItemResponseDto>(500, $"Error retrieving portfolio item: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get portfolio items count for a craftsman
        /// </summary>
        [HttpGet("craftsman/{craftsmanId}/count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<int>>> GetPortfolioCount(int craftsmanId)
        {
            try
            {
                if (craftsmanId <= 0)
                    return BadRequest(new ApiResponse<int>(400, "Invalid craftsman ID"));

                _logger.LogInformation("Getting portfolio count for craftsman {CraftsmanId}", craftsmanId);
                var count = await _portfolioService.GetCraftsmanPortfolioCountAsync(craftsmanId);
                return Ok(new ApiResponse<int>(200, "Portfolio count retrieved successfully", count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio count for craftsman {CraftsmanId}", craftsmanId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<int>(500, $"Error getting portfolio count: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new portfolio item with image upload
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PortfolioItemResponseDto>>> Create(
            [FromForm] int craftsmanId,
            [FromForm] string title,
            [FromForm] string? description,
            [FromForm] int displayOrder,
            IFormFile image)
        {
            try
            {
                if (craftsmanId <= 0)
                    return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, "Invalid craftsman ID"));

                if (image == null || image.Length == 0)
                    return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, "Portfolio image is required"));

                if (string.IsNullOrWhiteSpace(title))
                    return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, "Portfolio title is required"));

                _logger.LogInformation("Creating portfolio item for craftsman {CraftsmanId} with title: {Title}",
                    craftsmanId, title);

                // Upload image to Cloudinary
                string imageUrl;
                try
                {
                    imageUrl = await _fileUploadService.UploadFileAsync(image, "portfolio");
                    _logger.LogInformation("Portfolio image uploaded successfully: {ImageUrl}", imageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading portfolio image");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiResponse<PortfolioItemResponseDto>(500, $"Error uploading image: {ex.Message}"));
                }

                // Create portfolio item DTO
                var dto = new CreatePortfolioItemDto
                {
                    CraftsmanId = craftsmanId,
                    Title = title,
                    Description = description,
                    DisplayOrder = displayOrder
                };

                // Create portfolio item in database
                var createdItem = await _portfolioService.CreateAsync(dto, imageUrl);

                return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, 
                    new ApiResponse<PortfolioItemResponseDto>(201, "Portfolio item created successfully", createdItem));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craftsman not found");
                return NotFound(new ApiResponse<PortfolioItemResponseDto>(404, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during portfolio creation");
                return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portfolio item");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PortfolioItemResponseDto>(500, $"Error creating portfolio item: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update a portfolio item (can include new image)
        /// </summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PortfolioItemResponseDto>>> Update(
            int id,
            [FromForm] string title,
            [FromForm] string? description,
            [FromForm] int displayOrder,
            [FromForm] bool isActive,
            IFormFile? image)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, "Invalid portfolio item ID"));

                if (string.IsNullOrWhiteSpace(title))
                    return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, "Portfolio title is required"));

                _logger.LogInformation("Updating portfolio item {PortfolioItemId}", id);

                // Get existing item first
                var existingItem = await _portfolioService.GetByIdAsync(id);
                if (existingItem == null)
                    return NotFound(new ApiResponse<PortfolioItemResponseDto>(404, $"Portfolio item with ID {id} not found"));

                string? imageUrl = existingItem.ImageUrl;

                // Handle image replacement if new image provided
                if (image != null && image.Length > 0)
                {
                    try
                    {
                        // Delete old image
                        if (!string.IsNullOrWhiteSpace(existingItem.ImageUrl))
                        {
                            try
                            {
                                await _fileUploadService.DeleteFileAsync(existingItem.ImageUrl);
                                _logger.LogInformation("Old portfolio image deleted");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete old portfolio image, continuing");
                            }
                        }

                        // Upload new image
                        imageUrl = await _fileUploadService.UploadFileAsync(image, "portfolio");
                        _logger.LogInformation("New portfolio image uploaded: {ImageUrl}", imageUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading new portfolio image");
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new ApiResponse<PortfolioItemResponseDto>(500, $"Error uploading new image: {ex.Message}"));
                    }
                }

                // Create update DTO
                var updateDto = new UpdatePortfolioItemDto
                {
                    Id = id,
                    Title = title,
                    Description = description,
                    ImageUrl = imageUrl,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                };

                var updatedItem = await _portfolioService.UpdateAsync(updateDto);
                return Ok(new ApiResponse<PortfolioItemResponseDto>(200, "Portfolio item updated successfully", updatedItem));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Portfolio item not found for update");
                return NotFound(new ApiResponse<PortfolioItemResponseDto>(404, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during portfolio update");
                return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating portfolio item {PortfolioItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PortfolioItemResponseDto>(500, $"Error updating portfolio item: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a portfolio item and its image
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
                    return BadRequest(new ApiResponse<object>(400, "Invalid portfolio item ID"));

                _logger.LogInformation("Deleting portfolio item {PortfolioItemId}", id);

                // Delete from database and get image URL
                var imageUrl = await _portfolioService.DeleteAsync(id);

                if (imageUrl == null)
                    return NotFound(new ApiResponse<object>(404, $"Portfolio item with ID {id} not found"));

                // Delete image from Cloudinary
                try
                {
                    await _fileUploadService.DeleteFileAsync(imageUrl);
                    _logger.LogInformation("Portfolio image deleted from Cloudinary");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete portfolio image from Cloudinary, but database record deleted");
                    // Continue - database record is deleted even if cloud file deletion fails
                }

                return Ok(new ApiResponse<object>(200, "Portfolio item deleted successfully", new { imageUrl }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio item {PortfolioItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>(500, $"Error deleting portfolio item: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle portfolio item active/inactive status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PortfolioItemResponseDto>>> ToggleStatus(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, "Invalid portfolio item ID"));

                _logger.LogInformation("Toggling status for portfolio item {PortfolioItemId}", id);
                var updatedItem = await _portfolioService.ToggleActiveStatusAsync(id);
                return Ok(new ApiResponse<PortfolioItemResponseDto>(200, "Portfolio item status toggled successfully", updatedItem));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Portfolio item not found");
                return NotFound(new ApiResponse<PortfolioItemResponseDto>(404, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error");
                return BadRequest(new ApiResponse<PortfolioItemResponseDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling portfolio item status {PortfolioItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PortfolioItemResponseDto>(500, $"Error toggling portfolio item status: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reorder portfolio items for a craftsman
        /// Expects a dictionary of {itemId: displayOrder}
        /// </summary>
        [HttpPost("craftsman/{craftsmanId}/reorder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioItemResponseDto>>>> ReorderItems(
            int craftsmanId,
            [FromBody] Dictionary<int, int> itemIdDisplayOrderMap)
        {
            try
            {
                if (craftsmanId <= 0)
                    return BadRequest(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(400, "Invalid craftsman ID"));

                if (itemIdDisplayOrderMap == null || itemIdDisplayOrderMap.Count == 0)
                    return BadRequest(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(400, "Item display order map cannot be empty"));

                _logger.LogInformation("Reordering portfolio items for craftsman {CraftsmanId}", craftsmanId);
                var items = await _portfolioService.ReorderItemsAsync(craftsmanId, itemIdDisplayOrderMap);
                return Ok(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(200, "Portfolio items reordered successfully", items));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craftsman not found");
                return NotFound(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(404, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error");
                return BadRequest(new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(400, $"Validation error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering portfolio items");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<IEnumerable<PortfolioItemResponseDto>>(500, $"Error reordering portfolio items: {ex.Message}"));
            }
        }
    }
}
