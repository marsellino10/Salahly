using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using Salahly.DAL.Interfaces;
using Salahly.DAL.Entities;

namespace Salahly.DSL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            //SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            //_signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegistrationResponse> RegisterAsync(RegisterDto dto, string role)
        {
            try
            {
                var user = new ApplicationUser
                {
                    FullName = dto.FullName,
                    UserName = dto.UserName,
                    Email = dto.Email,
                    UserType = Enum.Parse<UserType>(role),
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    return new RegistrationResponse
                    {
                        Errors = result.Errors.Select(e => e.Description),
                        IsSuccess = false,
                    };
                }

                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }

                return new RegistrationResponse
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new RegistrationResponse
                {
                    IsSuccess = false,
                };
            }
        }

        public async Task<Tuple<ApplicationUser?, string?>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
                return null;

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            // create refresh token and persist
            var refreshToken = CreateRefreshToken(user.Id);
            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveAsync();

            // return token and refresh token in a tuple via a serialized string (format: access|refresh)
            var combined = token + "|" + refreshToken.Token;

            return new Tuple<ApplicationUser?,string?>(user,combined);
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("NameIdentifier", user.Id.ToString()),
            new Claim("Name", user.UserName ?? ""),
            new Claim("FullName", user.FullName ?? ""),
            new Claim("IsProfileCompleted", user.IsProfileCompleted.ToString()),
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken CreateRefreshToken(int userId)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Guid.NewGuid().ToString("N");
            return new RefreshToken
            {
                Token = token,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Revoked = false
            };
        }

        public async Task<Tuple<ApplicationUser?, string?>> RefreshAccessTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var stored = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
            if (stored == null || stored.Revoked || stored.ExpiresAt <= DateTime.UtcNow)
                return null;

            var user = stored.User;
            if (user == null) user = await _userManager.FindByIdAsync(stored.UserId.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var newAccess = GenerateJwtToken(user, roles);

            // Optionally rotate refresh token
            stored.Revoked = true;
            await _unitOfWork.RefreshTokens.RevokeAsync(stored);

            var newRefresh = CreateRefreshToken(user.Id);
            await _unitOfWork.RefreshTokens.AddAsync(newRefresh);
            await _unitOfWork.SaveAsync();

            var combined = newAccess + "|" + newRefresh.Token;
            return new Tuple<ApplicationUser?, string?>(user, combined);
        }
    }
}
