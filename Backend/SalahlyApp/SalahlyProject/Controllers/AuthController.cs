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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<RegistrationResponse>(400, "Validation failed", new RegistrationResponse { Errors = errors }));
            }
            var success = await _authService.RegisterAsync(dto, "Customer");
            if (!success.IsSuccess)
                return BadRequest(new ApiResponse<RegistrationResponse>(400, "Registration failed.", success));
            return Ok(new ApiResponse<RegistrationResponse>(200, "Customer registered successfully.", success));
        }

        [HttpPost("register-technician")]
        public async Task<IActionResult> RegisterTechnician([FromBody] RegisterDto dto)
        {
                        if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                var success1 = new RegistrationResponse { Errors = errors, IsSuccess = false };
                return BadRequest(new ApiResponse<RegistrationResponse>(400, "Validation failed", success1));
            }
            var success = await _authService.RegisterAsync(dto, "Craftsman");
            if (!success.IsSuccess)
                return BadRequest(new ApiResponse<RegistrationResponse>(400, "Registration failed.", success));
            return Ok(new ApiResponse<RegistrationResponse>(200,"Technician registered successfully.",success));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var UserTuple = await _authService.LoginAsync(dto);
            if (UserTuple is null || UserTuple.Item1 == null)
                return Unauthorized(new ApiResponse<string>(401, "Invalid credentials.", null));

            // split combined token string
            var parts = (UserTuple.Item2 ?? "").Split('|');
            var access = parts.Length > 0 ? parts[0] : null;
            var refresh = parts.Length > 1 ? parts[1] : null;

            return Ok(new ApiResponse<object>(200, "Login successfully.", new { IsProfileCompleted =  UserTuple.Item1.IsProfileCompleted,UserType = UserTuple.Item1.UserType.ToString(), Token = access, RefreshToken = refresh }));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRecord token)
        {
            if (string.IsNullOrWhiteSpace(token.refreshToken))
                return BadRequest(new ApiResponse<string>(400, "Refresh token is required", null));

            var tuple = await _authService.RefreshAccessTokenAsync(token.refreshToken);
            if (tuple is null || tuple.Item1 == null)
                return Unauthorized(new ApiResponse<string>(401, "Invalid or expired refresh token", null));

            var parts = (tuple.Item2 ?? "").Split('|');
            var access = parts.Length > 0 ? parts[0] : null;
            var refresh = parts.Length > 1 ? parts[1] : null;

            return Ok(new ApiResponse<object>(200, "Token refreshed", new { Token = access, RefreshToken = refresh }));
        }
    }

}
