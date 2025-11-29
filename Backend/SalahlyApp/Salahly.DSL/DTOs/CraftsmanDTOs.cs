using Salahly.DAL.Entities;
using Salahly.DSL.DTOs.PortfolioDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs
{
    public class CraftsmanDto
    {
        public int Id { get; set; }
        public int CraftId { get; set; }
        public string? CraftName { get; set; }
        public string? JobTitle { get; set; }
        public double RatingAverage { get; set; }
        public int TotalCompletedBookings { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Bio { get; set; }
        public int YearsOfExperience { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? FullName { get; set; }

        public IEnumerable<PortfolioItemResponseDto> Portfolio { get; set; } = new List<PortfolioItemResponseDto>();
        // Changed to use simplified ServiceAreaDto instead of CraftsmanServiceAreaDto
        public IEnumerable<ServiceAreaDto> ServiceAreas { get; set; } = new List<ServiceAreaDto>();
        public IEnumerable<CraftsmanReviewDto> Reviews { get; set; } = new List<CraftsmanReviewDto>();
    }

    public class CraftsmanReviewDto
    {
        public int Id { get; set; }
        public int ReviewerUserId { get; set; }
        public string ReviewerName { get; set; }
        public string? ReviewerProfileImageUrl { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BookingId { get; set; }
    }

    public class CreateCraftsmanDto
    {
        //[Required]
        public int UserId { get; set; }

        [Required]
        public int CraftId { get; set; }

        [Required]
        public string FullName { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Bio { get; set; }
        public int YearsOfExperience { get; set; }

        // Service areas referenced by AreaId (pre-existing Area entity)
        public IEnumerable<AddServiceAreaDto> ServiceAreas { get; set; } = new List<AddServiceAreaDto>();
    }

    public class UpdateCraftsmanDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int CraftId { get; set; }

        [Required]
        public string FullName { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Bio { get; set; }
        public int YearsOfExperience { get; set; }

        // Service areas referenced by AreaId (pre-existing Area entity)
        public IEnumerable<AddServiceAreaDto> ServiceAreas { get; set; } = new List<AddServiceAreaDto>();
    }

    public class AddServiceAreaDto
    {
        [Required]
        public int AreaId { get; set; }
        public int ServiceRadiusKm { get; set; } = 10;
    }

    /// <summary>
    /// Simplified DTO for displaying service area info in responses.
    /// Contains flattened Area info (Region/City) for convenience.
    /// </summary>
    public class ServiceAreaDto
    {
        public int? AreaId { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
        public int ServiceRadiusKm { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Full DTO for detailed service area operations (when Area navigation is needed).
    /// Includes nested AreaDto for complete Area information.
    /// </summary>
    public class CraftsmanServiceAreaDto
    {
        public int? AreaId { get; set; }
        public AreaDto? Area { get; set; }
        public int ServiceRadiusKm { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CraftsmanShortDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? CraftName { get; set; }
        public double RatingAverage { get; set; }
    }

    //public class PortfolioItemDto
    //{
    //    public int Id { get; set; }
    //    public string Title { get; set; }
    //    public string? Description { get; set; }
    //    public string ImageUrl { get; set; }
    //    public int DisplayOrder { get; set; }
    //    public bool IsActive { get; set; }
    //}

    //public class AddPortfolioItemDto
    //{
    //    [Required]
    //    public int CraftsmanId { get; set; }
    //    [Required]
    //    public string Title { get; set; }
    //    public string? Description { get; set; }
    //    [Required]
    //    public string ImageUrl { get; set; }
    //    public int DisplayOrder { get; set; }
    //}
}
