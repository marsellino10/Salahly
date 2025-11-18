using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using SalahlyProject.Response;
using SalahlyProject.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace SalahlyProject.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/servicerequests")]
    [Authorize(Roles = "Customer")]
    public class CustomerServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _service;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<CustomerServiceRequestController> _logger;

        public CustomerServiceRequestController(IServiceRequestService service, IFileUploadService fileUploadService, ILogger<CustomerServiceRequestController> logger)
        {
            _service = service;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ServiceRequestResponseDto>>> Create([FromForm] CreateServiceRequestDto dto,IFormFileCollection ImageFiles)
        {
            try
            {
                // extract customer id from token
                var customerClaim = User.FindFirst("NameIdentifier")?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new ApiResponse<ServiceRequestResponseDto>(401, "Customer ID not found in token"));
                if(ImageFiles != null)
                {
                    try
                    {
                        List<string> imageUrls = new List<string>();
                        foreach (var file in ImageFiles)
                        {
                            imageUrls.Add( await _fileUploadService.UploadFileAsync(file, "serviceRequest"));
                        }
                        dto.ImagesJson = JsonSerializer.Serialize(imageUrls);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading Service Reqeust image to Cloudinary for Customer ID: {customerClaim}", customerClaim);
                    }
                }
                var result = await _service.CreateAsync(dto, customerId);
                return CreatedAtAction(nameof(GetById), new { id = result.ServiceRequestId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ServiceRequestDto>>>> GetAll()
        {
            try
            {
                var customerClaim = User.FindFirst("NameIdentifier")?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new ApiResponse<IEnumerable<ServiceRequestDto>>(401, "Customer ID not found in token"));
                var result = await _service.GetAllByCustomerAsync(customerId);
                return Ok(new ApiResponse<IEnumerable<ServiceRequestDto>>(200, "Get All Successfully", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceRequestDto>>> GetById(int id)
        {
            try
            {
                var customerClaim = User.FindFirst("NameIdentifier")?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new ApiResponse<ServiceRequestDto>(401, "Customer ID not found in token",null));

                var result = await _service.GetByIdAsync(id, customerId);

                if (result == null)
                    return NotFound(new ApiResponse<ServiceRequestDto>(404, "Service request not found", null));

                return Ok(new ApiResponse<ServiceRequestDto>(200, "Get Successfully", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<ServiceRequestDto>(400, $"{ex.Message}", null));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customerClaim = User.FindFirst("NameIdentifier")?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new { message = "Customer ID not found in token" });
                var success = await _service.DeleteAsync(id, customerId);
                if (!success)
                    return NotFound(new { message = "Service request not found or could not be deleted" });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceRequestDto>>> Update(int id, [FromForm] UpdateServiceRequestDto dto)
        {
            try
            {
                var customerClaim = User.FindFirst("NameIdentifier")?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new { message = "Customer ID not found in token" });
                var request = await _service.UpdateAsync(id, dto, customerId);
                var result = await _service.GetByIdAsync(request.ServiceRequestId, customerId);
                return Ok(new ApiResponse<ServiceRequestDto>(200,"Updated Successfully", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}