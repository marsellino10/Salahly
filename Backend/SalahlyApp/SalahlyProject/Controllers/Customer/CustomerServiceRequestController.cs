using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using System.Security.Claims;

namespace SalahlyProject.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/servicerequests")]
    [Authorize(Roles = "Customer")]
    public class CustomerServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _service;

        public CustomerServiceRequestController(IServiceRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequestDto dto)
        {
            try
            {
                Console.WriteLine("dnsajnsaj");

                // extract customer id from token
                var customerClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new { message = "Customer ID not found in token" });

                var result = await _service.CreateAsync(dto, customerId);
                return CreatedAtAction(nameof(GetById), new { id = result.ServiceRequestId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customerClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new { message = "Customer ID not found in token" });
                var result = await _service.GetAllByCustomerAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var customerClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new { message = "Customer ID not found in token" });

                var result = await _service.GetByIdAsync(id, customerId);

                if (result == null)
                    return NotFound(new { message = "Service request not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customerClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequestDto dto)
        {
            try
            {
                var customerClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerClaim) || !int.TryParse(customerClaim, out var customerId))
                    return Unauthorized(new { message = "Customer ID not found in token" });
                var result = await _service.UpdateAsync(id, dto, customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}