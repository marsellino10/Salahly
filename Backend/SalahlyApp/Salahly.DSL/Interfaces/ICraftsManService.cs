using Salahly.DSL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    public interface ICraftsManService
    {
        Task<IEnumerable<CraftsmanDto>> GetAllAsync();
        Task<CraftsmanDto?> GetByIdAsync(int id);
        Task<CraftsmanDto> CreateAsync(CreateCraftsmanDto dto);
        Task<CraftsmanDto> UpdateAsync(UpdateCraftsmanDto dto);
        Task DeleteAsync(int id);

        Task<CraftsmanDto> UpdateCraftsManImageAsync(int craftsManId, string iconUrl);

        Task<PortfolioItemDto> AddPortfolioItemAsync(AddPortfolioItemDto dto);
        /// <summary>
        /// Deletes portfolio item and returns the image URL that was stored (so caller can delete from cloud)
        /// </summary>
        Task<string?> DeletePortfolioItemAsync(int portfolioItemId);
    }
}
