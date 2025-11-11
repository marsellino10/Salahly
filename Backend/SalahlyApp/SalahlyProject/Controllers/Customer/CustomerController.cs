using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using System.Security.Claims;

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
            var customerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _service.GetByIdAsync(id, customerId);
            if (result == null) return Unauthorized(new { message = "Access denied or customer not found." });
            return Ok(result);
        }

        // PUT api/customer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
        {
            var customerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _service.UpdateAsync(id, dto, customerId);
            if (result == null) return Unauthorized(new { message = "Access denied or customer not found." });
            return Ok(result);
        }
    }

}
