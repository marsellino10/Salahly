using System.ComponentModel.DataAnnotations;

namespace SalahlyProject.Contracts.Chat
{
    public class ChatRequestDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Question { get; set; } = string.Empty;

        public string? Context { get; set; }
    }
}
