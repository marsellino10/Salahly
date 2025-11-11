using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs
{
    /// <summary>
    /// DTO for creating a new craft
    /// </summary>
    public class CreateCraftDto
    {
        [Required(ErrorMessage = "Craft name is required")]
        [MaxLength(100, ErrorMessage = "Craft name cannot exceed 100 characters")]
        public string Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating an existing craft
    /// </summary>
    public class UpdateCraftDto
    {
        [Required(ErrorMessage = "Craft ID is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Craft name is required")]
        [MaxLength(100, ErrorMessage = "Craft name cannot exceed 100 characters")]
        public string Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for retrieving craft information (response)
    /// </summary>
    public class CraftDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Number of craftsmen working in this craft
        /// </summary>
        public int CraftsmenCount { get; set; }

        /// <summary>
        /// Number of active service requests for this craft
        /// </summary>
        public int ActiveServiceRequestsCount { get; set; }
    }
}
