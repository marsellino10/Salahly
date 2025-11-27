using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Salahly.DSL.DTOs;

namespace Salahly.DSL.DTOs.PortfolioDtos
{
    /// <summary>
    /// DTO for displaying portfolio item information
    /// </summary>
    public class PortfolioItemResponseDto
    {
        public int Id { get; set; }
        public int CraftsmanId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new portfolio item
    /// ImageUrl will be populated after file upload
    /// </summary>
    public class CreatePortfolioItemDto
    {
        [Required]
        public int CraftsmanId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// DTO for updating portfolio item
    /// ImageUrl can be updated to a new file URL
    /// </summary>
    public class UpdatePortfolioItemDto
    {
        [Required]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for bulk operations on portfolio items
    /// </summary>
    public class PortfolioItemsResponseDto
    {
        public IEnumerable<PortfolioItemResponseDto> Items { get; set; } = new List<PortfolioItemResponseDto>();
        public int TotalCount { get; set; }
    }

    public class PortfolioItemDetailsDto
    {
        public PortfolioItemResponseDto Item { get; set; }
        public IEnumerable<CraftsmanReviewDto> Reviews { get; set; } = new List<CraftsmanReviewDto>();
    }
}
