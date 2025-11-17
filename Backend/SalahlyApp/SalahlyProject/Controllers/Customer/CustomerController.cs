using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.CustomerDtos;
using Salahly.DSL.Interfaces;
using SalahlyProject.Response;
using SalahlyProject.Services;
using SalahlyProject.Services.Interfaces;
using System.Security.Claims;

namespace SalahlyProject.Controllers.Customer
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        private readonly IFileUploadService _fileUploadService;

        public CustomerController(ICustomerService service, IFileUploadService fileUploadService)
        {
            _service = service;
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        }

        // GET api/customer/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customerId = int.Parse(User.FindFirst("NameIdentifier")?.Value ?? "0");
            var result = await _service.GetByIdAsync(id, customerId);
            if (result == null) return Unauthorized(new { message = "Access denied or customer not found." });
            return Ok(result);
        }

        // PUT api/customer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] CustomerUpdateDto dto, IFormFile? ImageProfile)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("NameIdentifier")?.Value ?? "0");
                if (id <= 0)
                    return BadRequest(new ApiResponse<CraftsmanDto>(400, "Invalid craftsman ID"));

                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<CraftsmanDto>(400, "Invalid model state", null));
                // Handle profile image replacement if provided
                if (ImageProfile != null)
                {
                    try
                    {
                        // Get existing craftsman to retrieve old profile image URL
                        var existingCustomer= await _service.GetByIdAsync(customerId,id);
                        if (existingCustomer == null)
                            return NotFound(new ApiResponse<CraftsmanDto>(404, $"Craftsman with ID {id} not found"));

                        // Delete old profile image if it exists
                        if (!string.IsNullOrWhiteSpace(existingCustomer.ProfileImageUrl))
                        {
                            try
                            {
                                await _fileUploadService.DeleteFileAsync(existingCustomer.ProfileImageUrl);
                            }
                            catch (Exception ex)
                            {
                                // Continue without deleting old image
                            }
                        }

                        // Upload new profile image
                        var profileImageUrl = await _fileUploadService.UploadFileAsync(ImageProfile, "customer");
                        await _service.UpdateCustomerImageAsync(id, profileImageUrl);
                    }
                    catch (Exception ex)
                    {
                        // Continue without new profile image rather than failing the entire request
                    }
                }
                var result = await _service.UpdateAsync(id, dto, customerId);
                if (result == null) return Unauthorized(new { message = "Access denied or customer not found." });
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new ApiResponse<CustomerResponseDto>(400, $"Invalid input: {ex.Message}"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<CustomerResponseDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<CustomerResponseDto>(404, $"User not found: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CustomerResponseDto>(500, $"Error creating customer: {ex.Message}"));
            }
            
        }
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<CreateCustomerDto>>> CreateCustomer([FromForm] CreateCustomerDto dto, IFormFile? ImageProfile)
        {
            try
            {
                // Validate user and input
                var userId = await ValidateCustomerCreationAsync(dto);

                // Create the customer
                var createdCustomer = await _service.CreateAsync(dto);
                if (createdCustomer == null)
                {
                    return BadRequest(new ApiResponse<CustomerResponseDto>(400, "Failed to create customer"));
                }

                // Handle profile image if provided
                if (ImageProfile != null)
                {
                    var imageResult = await HandleProfileImageUploadAsync(createdCustomer.Id, ImageProfile);
                    if (imageResult.Success)
                    {
                        createdCustomer = imageResult.UpdatedCustomer;
                    }
                    // If image upload fails, continue without image (non-blocking failure)
                }

                return Ok(new ApiResponse<CustomerResponseDto>(201, "Customer created successfully", createdCustomer));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new ApiResponse<CustomerResponseDto>(400, $"Invalid input: {ex.Message}"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<CustomerResponseDto>(400, $"Validation error: {ex.Message}"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<CustomerResponseDto>(404, $"User not found: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<CustomerResponseDto>(500, $"Error creating customer: {ex.Message}"));
            }
        }

        /// <summary>
        /// Validates customer creation input and extracts user ID from token
        /// </summary>
        /// <param name="dto">The customer creation DTO</param>
        /// <returns>The validated user ID</returns>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        /// <exception cref="ArgumentException">Thrown when UserId is invalid or ModelState is invalid</exception>
        private async Task<int> ValidateCustomerCreationAsync(CreateCustomerDto dto)
        {
            // Null check for DTO
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Customer creation data cannot be null");
            }

            // Get UserId from token
            var userIdStr = User.FindFirstValue("NameIdentifier");
            if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
            {
                throw new ArgumentException("Invalid UserId in token", nameof(userId));
            }

            dto.UserId = userId;

            // Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                throw new ArgumentException($"Model validation failed: {string.Join(", ", errors)}", nameof(dto));
            }

            return userId;
        }

        /// <summary>
        /// Handles profile image upload to cloud storage and updates customer record
        /// Non-blocking: logs warnings but doesn't throw exceptions on failure
        /// </summary>
        /// <param name="customerId">The ID of the customer</param>
        /// <param name="imageFile">The image file to upload</param>
        /// <returns>An ImageUploadResult containing success status and updated customer data</returns>
        private async Task<ImageUploadResult> HandleProfileImageUploadAsync(int customerId, IFormFile imageFile)
        {
            var result = new ImageUploadResult { Success = false, UpdatedCustomer = null };

            if (imageFile == null || imageFile.Length == 0)
            {
                return result;
            }

            try
            {
                // Upload file to cloud storage
                var imageUrl = await _fileUploadService.UploadFileAsync(imageFile, "customers");

                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return result;
                }

                // Update customer with image URL
                var updatedCustomer = await _service.UpdateCustomerImageAsync(customerId, imageUrl);

                result.Success = true;
                result.UpdatedCustomer = updatedCustomer;

                return result;
            }
            catch (ArgumentException ex)
            {
                // File validation errors (size, extension, etc.)
                return result;
            }
            catch (InvalidOperationException ex)
            {
                // Cloudinary upload errors
                return result;
            }
            catch (KeyNotFoundException ex)
            {
                // Customer not found
                return result;
            }
            catch (Exception ex)
            {
                // Unexpected errors
                return result;
            }
        }

        /// <summary>
        /// Internal class to encapsulate image upload operation results
        /// </summary>
        private class ImageUploadResult
        {
            public bool Success { get; set; }
            public CustomerResponseDto? UpdatedCustomer { get; set; }
        }

    }

}
