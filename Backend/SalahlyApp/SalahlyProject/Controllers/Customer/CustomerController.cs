using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.CustomerDtos;
using Salahly.DSL.Interfaces;
using SalahlyProject.Response;

namespace SalahlyProject.Controllers.Customer
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service;
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
        public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
        {
            var customerId = int.Parse(User.FindFirst("NameIdentifier")?.Value ?? "0");
            var result = await _service.UpdateAsync(id, dto, customerId);
            if (result == null) return Unauthorized(new { message = "Access denied or customer not found." });
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<CreateCustomerDto>>> CreateCustomer([FromForm] CreateCustomerDto dto)
        {
            var userId = User.FindFirstValue("NameIdentifier");
            dto.UserId = userId;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<string>(400, string.Join(", ", errors), null));
            }

            var createdCustomer = await _service.CreateAsync(dto);

            if (createdCustomer == null)
            {
                return BadRequest(new ApiResponse<string>(400, "Failed to create customer", "false"));
            }

            return Ok(new ApiResponse<CustomerResponseDto>(200, "customer created successfully", createdCustomer));

        }

    }

}
