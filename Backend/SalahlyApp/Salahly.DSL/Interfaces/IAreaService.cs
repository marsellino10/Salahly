using Salahly.DSL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    public interface IAreaService
    {
        Task<IEnumerable<AreaDto>> GetAllAsync();
        Task<AreaDto?> GetByIdAsync(int id);
        Task<AreaDto> CreateAsync(CreateAreaDto dto);
        Task<AreaDto> UpdateAsync(UpdateAreaDto dto);
        Task DeleteAsync(int id);
    }
}
