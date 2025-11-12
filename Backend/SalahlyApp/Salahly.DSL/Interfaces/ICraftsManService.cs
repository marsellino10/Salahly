using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.PortfolioDtos;
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

    }
}
