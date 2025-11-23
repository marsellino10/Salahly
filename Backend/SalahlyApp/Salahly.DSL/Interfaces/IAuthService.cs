using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    // /Auth/Interfaces/IAuthService.cs
    using System.Threading.Tasks;
    using Salahly.DSL.DTOs;

    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterDto dto, string role);
        Task<Tuple<ApplicationUser?, string?>> LoginAsync(LoginDto dto);
        Task<Tuple<ApplicationUser?, string?>> RefreshAccessTokenAsync(string refreshToken);
    }

}
