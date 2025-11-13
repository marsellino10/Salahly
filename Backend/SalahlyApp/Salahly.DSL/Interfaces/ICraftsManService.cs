using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.PortfolioDtos;
using Salahly.DSL.Filters;
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

        /// <summary>
        /// Get all craftsmen with filtering and pagination support
        /// </summary>
        /// <param name="filter">Filter parameters including search, craft, area, and pagination</param>
        /// <returns>Paginated list of craftsmen matching the filters</returns>
        Task<PaginatedResponse<CraftsmanDto>> GetAllWithFiltersAsync(CraftsmanFilterDto filter);
    }
}
