using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using SalahlyProject.Response;

namespace SalahlyProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AreaController : ControllerBase
    {
        private readonly IAreaService _service;
        private readonly ILogger<AreaController> _logger;

        public AreaController(IAreaService service, ILogger<AreaController> logger)
        {
            _service = service;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all areas
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AreaDto>>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all areas");
                var list = await _service.GetAllAsync();
                return Ok(new ApiResponse<IEnumerable<AreaDto>>(200, "Areas retrieved successfully", list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving areas");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<IEnumerable<AreaDto>>(500, $"Error retrieving areas: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get an area by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AreaDto>>> Get(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<AreaDto>(400, "Invalid area ID"));

                _logger.LogInformation("Getting area with ID: {AreaId}", id);
                var item = await _service.GetByIdAsync(id);
                if (item == null)
                    return NotFound(new ApiResponse<AreaDto>(404, $"Area with ID {id} not found"));

                return Ok(new ApiResponse<AreaDto>(200, "Area retrieved successfully", item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving area with ID: {AreaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<AreaDto>(500, $"Error retrieving area: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new area
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AreaDto>>> Create([FromBody] CreateAreaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<AreaDto>(400, "Invalid model state", null));

                _logger.LogInformation("Creating area");
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, 
                    new ApiResponse<AreaDto>(201, "Area created successfully", created));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during area creation");
                return BadRequest(new ApiResponse<AreaDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating area");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<AreaDto>(500, $"Error creating area: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an existing area
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AreaDto>>> Update(int id, [FromBody] UpdateAreaDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<AreaDto>(400, "Invalid area ID"));

                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<AreaDto>(400, "Invalid model state", null));

                if (id != dto.Id)
                    return BadRequest(new ApiResponse<AreaDto>(400, "ID in URL does not match ID in request body"));

                _logger.LogInformation("Updating area with ID: {AreaId}", id);
                var updated = await _service.UpdateAsync(dto);
                return Ok(new ApiResponse<AreaDto>(200, "Area updated successfully", updated));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Area not found for update: {AreaId}", id);
                return NotFound(new ApiResponse<AreaDto>(404, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during area update");
                return BadRequest(new ApiResponse<AreaDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating area with ID: {AreaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<AreaDto>(500, $"Error updating area: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete an area
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
                    return BadRequest(new ApiResponse<object>(400, "Invalid area ID"));

                _logger.LogInformation("Deleting area with ID: {AreaId}", id);
                await _service.DeleteAsync(id);
                return Ok(new ApiResponse<object>(200, $"Area with ID {id} successfully deleted", new { id }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Area not found for deletion: {AreaId}", id);
                return NotFound(new ApiResponse<object>(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting area with ID: {AreaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>(500, $"Error deleting area: {ex.Message}"));
            }
        }
    }
}
