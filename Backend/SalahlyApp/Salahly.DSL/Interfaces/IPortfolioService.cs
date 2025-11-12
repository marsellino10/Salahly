using Salahly.DSL.DTOs.PortfolioDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    /// <summary>
    /// Service interface for portfolio item management
    /// Handles all portfolio operations for craftsmen
    /// </summary>
    public interface IPortfolioService
    {
        /// <summary>
        /// Get all portfolio items for a specific craftsman
        /// </summary>
        Task<IEnumerable<PortfolioItemResponseDto>> GetByCraftsmanAsync(int craftsmanId);

        /// <summary>
        /// Get a single portfolio item by ID
        /// </summary>
        Task<PortfolioItemResponseDto?> GetByIdAsync(int portfolioItemId);

        /// <summary>
        /// Create a new portfolio item
        /// </summary>
        Task<PortfolioItemResponseDto> CreateAsync(CreatePortfolioItemDto dto, string imageUrl);

        /// <summary>
        /// Update an existing portfolio item
        /// Returns the updated item
        /// </summary>
        Task<PortfolioItemResponseDto> UpdateAsync(UpdatePortfolioItemDto dto);

        /// <summary>
        /// Delete a portfolio item by ID
        /// Returns the image URL of the deleted item (so caller can delete from cloud storage)
        /// </summary>
        Task<string?> DeleteAsync(int portfolioItemId);

        /// <summary>
        /// Get portfolio items count for a craftsman
        /// </summary>
        Task<int> GetCraftsmanPortfolioCountAsync(int craftsmanId);

        /// <summary>
        /// Get active portfolio items for a craftsman
        /// </summary>
        Task<IEnumerable<PortfolioItemResponseDto>> GetActiveByCraftsmanAsync(int craftsmanId);

        /// <summary>
        /// Toggle portfolio item active status
        /// </summary>
        Task<PortfolioItemResponseDto> ToggleActiveStatusAsync(int portfolioItemId);

        /// <summary>
        /// Reorder portfolio items by display order
        /// </summary>
        Task<IEnumerable<PortfolioItemResponseDto>> ReorderItemsAsync(int craftsmanId, Dictionary<int, int> itemIdDisplayOrderMap);
    }
}
