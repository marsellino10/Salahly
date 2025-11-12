using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SalahlyProject.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
        /// Get all craftsmen
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CraftsmanDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all craftsmen");
                var list = await _service.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving craftsmen");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error retrieving craftsmen", error = ex.Message });
            }
        }

        /// <summary>
        /// Get craftsman by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CraftsmanDto>> Get(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid craftsman ID" });

                _logger.LogInformation("Getting craftsman with ID: {CraftsmanId}", id);
                var item = await _service.GetByIdAsync(id);
                if (item == null)
                    return NotFound(new { message = $"Craftsman with ID {id} not found" });

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving craftsman with ID: {CraftsmanId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error retrieving craftsman", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new craftsman with optional profile image and service areas
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CraftsmanDto>> Create([FromForm] CreateCraftsmanDto dto, IFormFile? profileImage, [FromForm] string? serviceAreasJson)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Creating craftsman with name: {FullName}", dto.FullName);

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
                        return BadRequest(new { message = "Invalid JSON format for serviceAreasJson", error = ex.Message });
                    }
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

                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid input for craftsman creation");
                return BadRequest(new { message = "Invalid input", error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during craftsman creation");
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating craftsman");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error creating craftsman", error = ex.Message });
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
        public async Task<ActionResult<CraftsmanDto>> Update(int id, [FromForm] UpdateCraftsmanDto dto, IFormFile? profileImage, [FromForm] string? serviceAreasJson)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid craftsman ID" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != dto.Id)
                    return BadRequest(new { message = "ID in URL does not match ID in request body" });

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
                        return BadRequest(new { message = "Invalid JSON format for serviceAreasJson", error = ex.Message });
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
                            return NotFound(new { message = $"Craftsman with ID {id} not found" });

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
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craftsman not found for update: {CraftsmanId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during craftsman update");
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating craftsman with ID: {CraftsmanId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error updating craftsman", error = ex.Message });
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
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid craftsman ID" });

                _logger.LogInformation("Deleting craftsman with ID: {CraftsmanId}", id);

                // Get existing craftsman to retrieve profile and portfolio images
                var existingCraftsman = await _service.GetByIdAsync(id);
                if (existingCraftsman == null)
                    return NotFound(new { message = $"Craftsman with ID {id} not found" });

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

                return Ok(new { message = $"Craftsman with ID {id} successfully deleted" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craftsman not found for deletion: {CraftsmanId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting craftsman with ID: {CraftsmanId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error deleting craftsman", error = ex.Message });
            }
        }

        /// <summary>
        /// Add a portfolio item to a craftsman
        /// </summary>
        [HttpPost("portfolio")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioItemDto>> AddPortfolio(
            [FromForm] int craftsmanId,
            [FromForm] string title,
            [FromForm] string? description,
            [FromForm] int displayOrder,
            IFormFile image)
        {
            try
            {
                if (craftsmanId <= 0)
                    return BadRequest(new { message = "Invalid craftsman ID" });

                if (image == null)
                    return BadRequest(new { message = "Portfolio image is required" });

                if (string.IsNullOrWhiteSpace(title))
                    return BadRequest(new { message = "Portfolio title is required" });

                _logger.LogInformation("Adding portfolio item for craftsman ID: {CraftsmanId}", craftsmanId);

                // Upload portfolio image to Cloudinary
                var imageUrl = await _fileUploadService.UploadFileAsync(image, "portfolio");
                _logger.LogInformation("Portfolio image uploaded: {Url}", imageUrl);

                // Create portfolio DTO with uploaded image URL
                var dto = new AddPortfolioItemDto
                {
                    CraftsmanId = craftsmanId,
                    Title = title,
                    Description = description,
                    ImageUrl = imageUrl,
                    DisplayOrder = displayOrder
                };

                var item = await _service.AddPortfolioItemAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = craftsmanId }, item);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craftsman not found for portfolio addition");
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error when adding portfolio item");
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding portfolio item");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error adding portfolio item", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a portfolio item and its image
        /// </summary>
        [HttpDelete("portfolio/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeletePortfolio(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid portfolio item ID" });

                _logger.LogInformation("Deleting portfolio item with ID: {PortfolioItemId}", id);

                var url = await _service.DeletePortfolioItemAsync(id);
                if (url == null)
                    return NotFound(new { message = $"Portfolio item with ID {id} not found" });

                // Delete image from cloud storage
                try
                {
                    await _fileUploadService.DeleteFileAsync(url);
                    _logger.LogInformation("Portfolio image deleted from Cloudinary: {Url}", url);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete portfolio image from Cloudinary: {Url}", url);
                    // Continue without failing - DB record is already deleted
                }

                return Ok(new { message = "Portfolio item deleted successfully", imageUrl = url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio item with ID: {PortfolioItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error deleting portfolio item", error = ex.Message });
            }
        }
    }
}
