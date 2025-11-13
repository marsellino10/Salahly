using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using SalahlyProject.Services.Interfaces;
using SalahlyProject.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalahlyProject.Controllers
{
    /// <summary>
    /// API Controller for Craft management
    /// Handles HTTP requests and delegates business logic to ICraftService
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CraftsController : ControllerBase
    {
        private readonly ICraftService _craftService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<CraftsController> _logger;

        public CraftsController(ICraftService craftService, IFileUploadService fileUploadService, ILogger<CraftsController> logger)
        {
            _craftService = craftService ?? throw new ArgumentNullException(nameof(craftService));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all crafts with optional filtering and pagination
        /// </summary>
        /// <param name="isActiveOnly">Filter to show only active crafts</param>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of records per page (default: 10)</param>
        /// <returns>Paginated list of crafts</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CraftDto>>>> GetAllCrafts([FromQuery] bool isActiveOnly = false)
        {
            try
            {
                _logger.LogInformation("Getting all crafts with filters - IsActive: {IsActive}",isActiveOnly);

                var result = await _craftService.GetAllCraftsAsync(isActiveOnly);
                return Ok(new ApiResponse<IEnumerable<CraftDto>>(200, "Crafts retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving crafts");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiResponse<IEnumerable<CraftDto>>(500, $"Error retrieving crafts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all active crafts for display (no pagination)
        /// Useful for dropdown lists or UI selection
        /// </summary>
        /// <returns>List of active crafts</returns>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CraftDto>>>> GetActiveCrafts()
        {
            try
            {
                _logger.LogInformation("Getting active crafts for display");
                var result = await _craftService.GetActiveCraftsForDisplayAsync();
                return Ok(new ApiResponse<IEnumerable<CraftDto>>(200, "Active crafts retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active crafts");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<IEnumerable<CraftDto>>(500, $"Error retrieving active crafts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific craft by ID
        /// </summary>
        /// <param name="id">Craft ID</param>
        /// <returns>Craft details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CraftDto>>> GetCraftById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<CraftDto>(400, "Invalid craft ID"));

                _logger.LogInformation("Getting craft with ID: {CraftId}", id);
                var craft = await _craftService.GetCraftByIdAsync(id);

                if (craft == null)
                    return NotFound(new ApiResponse<CraftDto>(404, $"Craft with ID {id} not found"));

                return Ok(new ApiResponse<CraftDto>(200, "Craft retrieved successfully", craft));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving craft with ID: {CraftId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CraftDto>(500, $"Error retrieving craft: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a craft by name
        /// </summary>
        /// <param name="name">Craft name</param>
        /// <returns>Craft details</returns>
        [HttpGet("by-name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CraftDto>>> GetCraftByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new ApiResponse<CraftDto>(400, "Craft name cannot be empty"));

                _logger.LogInformation("Getting craft by name: {CraftName}", name);
                var craft = await _craftService.GetCraftByNameAsync(name);

                if (craft == null)
                    return NotFound(new ApiResponse<CraftDto>(404, $"Craft with name '{name}' not found"));

                return Ok(new ApiResponse<CraftDto>(200, "Craft retrieved successfully", craft));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving craft by name: {CraftName}", name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CraftDto>(500, $"Error retrieving craft: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new craft with optional icon upload to Cloudinary
        /// </summary>
        /// <param name="createCraftDto">Craft data to create</param>
        /// <param name="iconFile">Optional craft icon image file</param>
        /// <returns>Created craft details</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<CraftDto>>> CreateCraft([FromForm] CreateCraftDto createCraftDto, IFormFile? iconFile)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<CraftDto>(400, "Invalid model state", null));

                _logger.LogInformation("Creating craft: {CraftName}", createCraftDto.Name);
                var createdCraft = await _craftService.CreateCraftAsync(createCraftDto);

                // Handle icon upload to Cloudinary if provided
                if (iconFile != null)
                {
                    try
                    {
                        var iconUrl = await _fileUploadService.UploadFileAsync(iconFile, "crafts");
                        createdCraft = await _craftService.UpdateCraftIconAsync(createdCraft.Id, iconUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading craft icon to Cloudinary for craft ID: {CraftId}", createdCraft.Id);
                        // Continue without icon rather than failing the entire request
                    }
                }

                return CreatedAtAction(nameof(GetCraftById), new { id = createdCraft.Id }, 
                    new ApiResponse<CraftDto>(201, "Craft created successfully", createdCraft));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict while creating craft: {CraftName}", createCraftDto.Name);
                return Conflict(new ApiResponse<CraftDto>(409, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating craft: {CraftName}", createCraftDto.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CraftDto>(500, $"Error creating craft: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an existing craft with optional icon replacement on Cloudinary
        /// </summary>
        /// <param name="id">Craft ID</param>
        /// <param name="updateCraftDto">Updated craft data</param>
        /// <param name="iconFile">Optional new craft icon image file to replace existing one</param>
        /// <returns>Updated craft details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<CraftDto>>> UpdateCraft(int id, [FromForm] UpdateCraftDto updateCraftDto, IFormFile? iconFile)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<CraftDto>(400, "Invalid craft ID"));

                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<CraftDto>(400, "Invalid model state", null));

                if (updateCraftDto.Id != id)
                    return BadRequest(new ApiResponse<CraftDto>(400, "ID in URL does not match ID in request body"));

                _logger.LogInformation("Updating craft with ID: {CraftId}", id);
                var updatedCraft = await _craftService.UpdateCraftAsync(updateCraftDto);

                // Handle icon upload/replacement if provided
                if (iconFile != null)
                {
                    try
                    {
                        // Get existing craft to retrieve old icon URL for deletion
                        var existingCraft = await _craftService.GetCraftByIdAsync(id);
                        
                        // Delete old icon if it exists
                        if (!string.IsNullOrWhiteSpace(existingCraft?.IconUrl))
                        {
                            try
                            {
                                await _fileUploadService.DeleteFileAsync(existingCraft.IconUrl);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete old icon for craft ID: {CraftId}", id);
                                // Continue without deleting old icon
                            }
                        }
                        
                        // Upload new icon to Cloudinary
                        var iconUrl = await _fileUploadService.UploadFileAsync(iconFile, "crafts");
                        updatedCraft = await _craftService.UpdateCraftIconAsync(id, iconUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading craft icon to Cloudinary for craft ID: {CraftId}", id);
                        // Continue without new icon rather than failing the entire request
                    }
                }

                return Ok(new ApiResponse<CraftDto>(200, "Craft updated successfully", updatedCraft));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craft not found for update: {CraftId}", id);
                return NotFound(new ApiResponse<CraftDto>(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict while updating craft: {CraftId}", id);
                return Conflict(new ApiResponse<CraftDto>(409, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating craft with ID: {CraftId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CraftDto>(500, $"Error updating craft: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a craft
        /// </summary>
        /// <param name="id">Craft ID to delete</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCraft(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<object>(400, "Invalid craft ID"));
                var existingCraft = await _craftService.GetCraftByIdAsync(id);
                _logger.LogInformation("Deleting craft with ID: {CraftId}", id);
                await _craftService.DeleteCraftAsync(id);
                await _fileUploadService.DeleteFileAsync(existingCraft.IconUrl ?? string.Empty);

                return Ok(new ApiResponse<object>(200, $"Craft with ID {id} successfully deleted", new { id }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craft not found for deletion: {CraftId}", id);
                return NotFound(new ApiResponse<object>(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete craft due to business rule: {CraftId}", id);
                return Conflict(new ApiResponse<object>(409, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting craft with ID: {CraftId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>(500, $"Error deleting craft: {ex.Message}"));
            }
        }

        /// <summary>
        /// Activate or deactivate a craft
        /// </summary>
        /// <param name="id">Craft ID</param>
        /// <param name="isActive">True to activate, false to deactivate</param>
        /// <returns>Updated craft details</returns>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CraftDto>>> ToggleCraftStatus(int id, [FromQuery] bool isActive)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<CraftDto>(400, "Invalid craft ID"));

                _logger.LogInformation("Toggling craft status - ID: {CraftId}, IsActive: {IsActive}", id, isActive);
                var updatedCraft = await _craftService.ToggleCraftActiveStatusAsync(id, isActive);

                return Ok(new ApiResponse<CraftDto>(200, "Craft status toggled successfully", updatedCraft));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Craft not found for status toggle: {CraftId}", id);
                return NotFound(new ApiResponse<CraftDto>(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling craft status: {CraftId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CraftDto>(500, $"Error updating craft status: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check if a craft name is unique
        /// </summary>
        /// <param name="name">Craft name to check</param>
        /// <param name="excludeId">Optional craft ID to exclude from uniqueness check</param>
        /// <returns>Boolean indicating if name is unique</returns>
        [HttpGet("check-name-unique")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> CheckNameUnique([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new ApiResponse<object>(400, "Craft name cannot be empty"));

                _logger.LogInformation("Checking craft name uniqueness: {CraftName}", name);
                var isUnique = await _craftService.IsCraftNameUniqueAsync(name, excludeId);

                return Ok(new ApiResponse<object>(200, isUnique ? "Name is available" : "Name is already taken", new { isUnique }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking craft name uniqueness");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>(500, $"Error checking name uniqueness: {ex.Message}"));
            }
        }
    }
}
