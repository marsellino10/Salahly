using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;

namespace SalahlyProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDto dto)
        {
            var success = await _authService.RegisterAsync(dto, "Customer");
            if (!success)
                return BadRequest("Registration failed.");
            return Ok("Customer registered successfully.");
        }

        [HttpPost("register-technician")]
        public async Task<IActionResult> RegisterTechnician([FromBody] RegisterDto dto)
        {
            var success = await _authService.RegisterAsync(dto, "Craftsman");
            if (!success)
                return BadRequest("Registration failed.");
            return Ok("Technician registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            if (token == null)
                return Unauthorized("Invalid credentials.");
            return Ok(token);
        }
    }

}
