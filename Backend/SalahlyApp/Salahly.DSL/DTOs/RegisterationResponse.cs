using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs
{
    public class RegistrationResponse
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public IEnumerable<string>? Errors { get; set; } = new List<string>();
        public bool IsSuccess { get; set; }
    }
}
