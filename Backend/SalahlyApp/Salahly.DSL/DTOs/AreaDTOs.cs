using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Salahly.DSL.DTOs
{
    public class AreaDto
    {
        public int Id { get; set; }
        public string Region { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    public class CreateAreaDto
    {
        [Required]
        public string Region { get; set; }
        [Required]
        public string City { get; set; }
    }

    public class UpdateAreaDto : CreateAreaDto
    {
        [Required]
        public int Id { get; set; }
    }
}
