using System;

namespace Salahly.DAL.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; } = false;
        public string? RemoteIpAddress { get; set; }
    }
}