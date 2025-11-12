using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.PortfolioDtos;
using Salahly.DSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salahly.DSL.Services
{
    public class CraftsManService : ICraftsManService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CraftsManService> _logger;

        public CraftsManService(IUnitOfWork unitOfWork, ILogger<CraftsManService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CraftsmanDto>> GetAllAsync()
        {
            var query = await _unitOfWork.Craftsmen.GetAllAsync();
            var list = await query
                .Include(c => c.User)
                .Include(c => c.Portfolio)
                .Include(c => c.CraftsmanServiceAreas).ThenInclude(sa => sa.Area)
                .ToListAsync();

            return list.Select(c => MapToDto(c));
        }

        public async Task<CraftsmanDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var query = await _unitOfWork.Craftsmen.GetAllAsync();
            var craftsman = await query
                .Include(c => c.User)
                .Include(c => c.Portfolio)
                .Include(c => c.CraftsmanServiceAreas).ThenInclude(sa => sa.Area)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (craftsman == null) return null;
            return MapToDto(craftsman);
        }

        public async Task<CraftsmanDto> CreateAsync(CreateCraftsmanDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Map DTO to entity
            var craftsman = new Craftsman
            {
                CraftId = dto.CraftId,
                Bio = dto.Bio,
                YearsOfExperience = dto.YearsOfExperience,
                HourlyRate = dto.HourlyRate,
                RatingAverage = 0,
                TotalCompletedBookings = 0,
                IsAvailable = true
            };

            await _unitOfWork.Craftsmen.AddAsync(craftsman);
            await _unitOfWork.SaveAsync();

            // Add service areas
            foreach (var area in dto.ServiceAreas ?? Enumerable.Empty<AddServiceAreaDto>())
            {
                var sa = new CraftsmanServiceArea
                {
                    CraftsmanId = craftsman.Id,
                    AreaId = area.AreaId,
                    ServiceRadiusKm = area.ServiceRadiusKm,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CraftsmanServiceAreas.AddAsync(sa);
            }

            await _unitOfWork.SaveAsync();

            return await GetByIdAsync(craftsman.Id)!
                   ?? throw new InvalidOperationException("Failed to retrieve created craftsman");
        }

        public async Task<CraftsmanDto> UpdateAsync(UpdateCraftsmanDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _unitOfWork.Craftsmen.GetByIdAsync(dto.Id);
            if (existing == null) throw new KeyNotFoundException($"Craftsman with ID {dto.Id} not found.");

            // Update scalar properties
            existing.CraftId = dto.CraftId;
            existing.Bio = dto.Bio;
            existing.YearsOfExperience = dto.YearsOfExperience;
            existing.HourlyRate = dto.HourlyRate;

            await _unitOfWork.Craftsmen.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();

            // Replace service areas: delete existing and add new
            var existingAreas = (await _unitOfWork.CraftsmanServiceAreas.GetAllAsync())
                .Where(a => a.CraftsmanId == existing.Id)
                .ToList();

            //update service areas Later
            foreach (var ea in existingAreas)
            {
                await _unitOfWork.CraftsmanServiceAreas.DeleteAsync(ea);
            }

            foreach (var area in dto.ServiceAreas ?? Enumerable.Empty<AddServiceAreaDto>())
            {
                var sa = new CraftsmanServiceArea
                {
                    CraftsmanId = existing.Id,
                    AreaId = area.AreaId,
                    ServiceRadiusKm = area.ServiceRadiusKm,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CraftsmanServiceAreas.AddAsync(sa);
            }

            await _unitOfWork.SaveAsync();

            return await GetByIdAsync(existing.Id)!
                   ?? throw new InvalidOperationException("Failed to retrieve updated craftsman");
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid craftsman ID", nameof(id));

            var craftsman = await _unitOfWork.Craftsmen.GetByIdAsync(id);
            if (craftsman == null) throw new KeyNotFoundException($"Craftsman with ID {id} not found.");

            // Delete portfolio items
            var portfolio = (await _unitOfWork.PortfolioItems.GetAllAsync())
                .Where(p => p.CraftsmanId == id)
                .ToList();
            foreach (var p in portfolio)
            {
                await _unitOfWork.PortfolioItems.DeleteAsync(p);
            }

            // Remove service areas
            var areas = (await _unitOfWork.CraftsmanServiceAreas.GetAllAsync())
                .Where(a => a.CraftsmanId == id)
                .ToList();
            foreach (var a in areas)
            {
                await _unitOfWork.CraftsmanServiceAreas.DeleteAsync(a);
            }

            // Delete craftsman
            await _unitOfWork.Craftsmen.DeleteAsync(craftsman);
            await _unitOfWork.SaveAsync();
        }


        /// <summary>
        /// Updates the craftsman profile image by persisting it to the associated user
        /// </summary>
        public async Task<CraftsmanDto> UpdateCraftsManImageAsync(int craftsmanId, string profileImageUrl)
        {
            if (craftsmanId <= 0)
                throw new ArgumentException("Invalid craftsman ID", nameof(craftsmanId));

            if (string.IsNullOrWhiteSpace(profileImageUrl))
                throw new ArgumentException("Profile image URL cannot be empty", nameof(profileImageUrl));

            var craftsman = await GetByIdWithIncludesAsync(craftsmanId);
            if (craftsman == null)
                throw new KeyNotFoundException($"Craftsman with ID {craftsmanId} not found.");

            // Update user profile image if user exists
            if (craftsman.User != null)
            {
                craftsman.User.ProfileImageUrl = profileImageUrl;
                await _unitOfWork.ApplicationUsers.UpdateAsync(craftsman.User);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Profile image updated for craftsman ID: {CraftsmanId}", craftsmanId);
            }
            else
            {
                _logger.LogWarning("No associated user found for craftsman ID: {CraftsmanId}", craftsmanId);
            }

            return await GetByIdAsync(craftsmanId)!
                   ?? throw new InvalidOperationException("Failed to retrieve updated craftsman");
        }

        // Helper mapper
        private CraftsmanDto MapToDto(Craftsman c)
        {
            var dto = c.Adapt<CraftsmanDto>();
            // Mapster now handles all mapping including User and ServiceAreas
            return dto;
        }
        //helper getWithIncludes
        private async Task<Craftsman?> GetByIdWithIncludesAsync(int id)
        {
            if (id <= 0) return null;
            var query = await _unitOfWork.Craftsmen.GetAllAsync();
            var craftsman = await query
                .Include(c => c.User)
                .Include(c => c.Portfolio)
                .Include(c => c.CraftsmanServiceAreas).ThenInclude(sa => sa.Area)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (craftsman == null) return null;
            return craftsman;
        }

    }
}
