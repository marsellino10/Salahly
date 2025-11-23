using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public string UserName { get; set; } = null!;
        public int UserId { get; set; }
    }
    public record RefreshTokenRecord
    {
        public string refreshToken { get; set; }
    }
}
