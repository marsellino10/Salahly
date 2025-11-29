using Salahly.DAL.Entities;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.PortfolioDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    public class AreaStatsDto
    {
        public int AreaId { get; set; }
        public string Region { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int RequestCount { get; set; }
    }

    public class CraftAverageReviewDto
    {
        public int CraftId { get; set; }
        public string CraftName { get; set; } = string.Empty;
        public double? AverageReview { get; set; }
    }

    public class OffersStatsDto
    {
        public int TotalOffers { get; set; }
        public double AverageOffersPerServiceRequest { get; set; }
    }

    public interface IAdminService
    {
        // Service Requests
        Task<int> CountServiceRequestsAsync(DateTime? from = null, DateTime? to = null, int? craftId = null, int? areaId = null);
        Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsFilteredAsync(DateTime? from = null, DateTime? to = null, int? craftId = null, int? areaId = null, string orderBy = "date", bool asc = false);
        Task<AreaStatsDto?> GetMostActiveAreaAsync();

        // Offers
        Task<OffersStatsDto> GetOffersStatsAsync();

        // Craftsmen
        Task<int> CountCraftsmenAsync(int? craftId = null, int? areaId = null);
        Task<int> GetTotalCraftsmenExperienceAsync(int? craftId = null, int? areaId = null);
        Task<IEnumerable<Salahly.DSL.DTOs.CraftsmanShortDto>> GetTopCraftsmenByReviewsAsync(int top = 5);

        // Crafts
        Task<int> CountCraftsAsync();
        Task<IEnumerable<CraftAverageReviewDto>> GetCraftsAverageReviewsAsync();

        // Portfolio
        Task<bool> ApprovePortfolioItemAsync(int portfolioItemId);
        Task<IEnumerable<PortfolioItemResponseDto>> GetInactivePortfolioItemsAsync();
    }
}
