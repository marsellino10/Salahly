using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using SalahlyProject.Response;

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
                return BadRequest(new ApiResponse<string>(400, "Registration failed.", null));
            return Ok(new ApiResponse<string>(200, "Customer registered successfully.", null));
        }

        [HttpPost("register-technician")]
        public async Task<IActionResult> RegisterTechnician([FromBody] RegisterDto dto)
        {
            var success = await _authService.RegisterAsync(dto, "Craftsman");
            if (!success)
                return BadRequest(new ApiResponse<string>(400, "Registration failed.", null));
            return Ok(new ApiResponse<string>(200,"Technician registered successfully.",null));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var UserTuple = await _authService.LoginAsync(dto);
            if (UserTuple is null || UserTuple.Item1 == null)
                return Unauthorized(new ApiResponse<string>(401, "Invalid credentials.", null));
            return Ok(new ApiResponse<object>(200, "Login successfully.", new { IsProfileCompleted =  UserTuple.Item1.IsProfileCompleted,UserType = UserTuple.Item1.UserType.ToString(),Token = UserTuple.Item2 }));
        }
    }

}
