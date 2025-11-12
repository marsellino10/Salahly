using Mapster;
using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salahly.DSL.Services
{
    public class AreaService : IAreaService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AreaService> _logger;

        public AreaService(IUnitOfWork uow, ILogger<AreaService> logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AreaDto> CreateAsync(CreateAreaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = new Area
            {
                Region = dto.Region,
                City = dto.City
            };

            await _uow.Areas.AddAsync(entity);
            await _uow.SaveAsync();

            return entity.Adapt<AreaDto>();
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id", nameof(id));
            var entity = await _uow.Areas.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException($"Area with id {id} not found.");

            // Remove related CraftsmanServiceAreas references
            var csas = (await _uow.CraftsmanServiceAreas.GetAllAsync()).Where(c => c.AreaId == id).ToList();
            foreach (var c in csas)
            {
                // unlink to preserve historical entries; could also delete
                c.AreaId = null;
                await _uow.CraftsmanServiceAreas.UpdateAsync(c);
            }

            await _uow.Areas.DeleteAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task<IEnumerable<AreaDto>> GetAllAsync()
        {
            var query = await _uow.Areas.GetAllAsync();
            var list = query.ToList();
            return list.Select(a => a.Adapt<AreaDto>());
        }

        public async Task<AreaDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _uow.Areas.GetByIdAsync(id);
            if (entity == null) return null;
            return entity.Adapt<AreaDto>();
        }

        public async Task<AreaDto> UpdateAsync(UpdateAreaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var entity = await _uow.Areas.GetByIdAsync(dto.Id);
            if (entity == null) throw new KeyNotFoundException($"Area with id {dto.Id} not found.");

            entity.Region = dto.Region;
            entity.City = dto.City;

            await _uow.Areas.UpdateAsync(entity);
            await _uow.SaveAsync();

            return entity.Adapt<AreaDto>();
        }
    }
}
