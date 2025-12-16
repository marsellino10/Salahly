using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Services
{
    /// <summary>
    /// Service implementation for Craft business logic
    /// Handles validation, transformation, and business rule enforcement
    /// </summary>
    public class CraftService : ICraftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CraftService> _logger;

        public CraftService(IUnitOfWork unitOfWork, ILogger<CraftService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieve all crafts with optional filtering and pagination
        /// </summary>
        public async Task<IEnumerable<CraftDto>> GetAllCraftsAsync(bool isActiveOnly = false)
        {

            // Query crafts with related data
            var query = await _unitOfWork.Crafts.GetAllAsync();

            // Apply active filter if requested
            if (isActiveOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            // Get total count
            var totalCount = await query.CountAsync();
            var crafts = await query.ToListAsync();
            // Map to DTOs and enrich with counts
            var craftDtos = await MapCraftsWithCountsAsync(crafts);

            return craftDtos;
        }

        /// <summary>
        /// Retrieve a single craft by ID with additional details
        /// </summary>
        public async Task<CraftDto?> GetCraftByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Craft ID must be greater than zero", nameof(id));

            var craftsQuery = await _unitOfWork.Crafts.GetAllAsync();
            var craft = await craftsQuery.FirstOrDefaultAsync(c => c.Id == id);

            if (craft == null)
                return null;

            return await MapCraftWithCountsAsync(craft);
        }

        /// <summary>
        /// Retrieve a craft by name
        /// </summary>
        public async Task<CraftDto?> GetCraftByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Craft name cannot be empty", nameof(name));

            var craftsQuery = await _unitOfWork.Crafts.GetAllAsync();
            var craft = await craftsQuery.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

            if (craft == null)
                return null;

            return await MapCraftWithCountsAsync(craft);
        }

        /// <summary>
        /// Create a new craft with validation
        /// </summary>
        public async Task<CraftDto> CreateCraftAsync(CreateCraftDto createCraftDto)
        {
            if (createCraftDto == null)
                throw new ArgumentNullException(nameof(createCraftDto));

            // Validate craft name is unique
            var craftsQuery = await _unitOfWork.Crafts.GetAllAsync();
            var nameExists = await craftsQuery
                .AnyAsync(c => c.Name.ToLower() == createCraftDto.Name.ToLower());

            if (nameExists)
                throw new InvalidOperationException($"A craft with the name '{createCraftDto.Name}' already exists.");

            // Create new craft entity
            var craft = new Craft
            {
                Name = createCraftDto.Name.Trim(),
                NameAr = createCraftDto.NameAr?.Trim(),
                Description = createCraftDto.Description?.Trim(),
                IconUrl = null,
                DisplayOrder = createCraftDto.DisplayOrder,
                IsActive = createCraftDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            // Add and save
            await _unitOfWork.Crafts.AddAsync(craft);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Craft created successfully: {CraftName} with ID: {CraftId}", craft.Name, craft.Id);
            return craft.Adapt<CraftDto>();
        }

        /// <summary>
        /// Update an existing craft with validation
        /// </summary>
        public async Task<CraftDto> UpdateCraftAsync(UpdateCraftDto updateCraftDto)
        {
            if (updateCraftDto == null)
                throw new ArgumentNullException(nameof(updateCraftDto));

            // Retrieve existing craft
            var craft = await _unitOfWork.Crafts.GetByIdAsync(updateCraftDto.Id);

            if (craft == null)
                throw new KeyNotFoundException($"Craft with ID {updateCraftDto.Id} not found.");

            // Validate craft name is unique (excluding current craft)
            var craftsQuery = await _unitOfWork.Crafts.GetAllAsync();
            var nameExists = await craftsQuery
                .AnyAsync(c => c.Id != updateCraftDto.Id &&
                         c.Name.ToLower() == updateCraftDto.Name.ToLower());

            if (nameExists)
                throw new InvalidOperationException($"A craft with the name '{updateCraftDto.Name}' already exists.");

            // Update craft properties
            craft.Name = updateCraftDto.Name.Trim();
            craft.NameAr = updateCraftDto.NameAr?.Trim();
            craft.Description = updateCraftDto.Description?.Trim();
            craft.DisplayOrder = updateCraftDto.DisplayOrder;
            craft.IsActive = updateCraftDto.IsActive;

            // Update and save
            await _unitOfWork.Crafts.UpdateAsync(craft);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Craft updated successfully: {CraftName} with ID: {CraftId}", craft.Name, craft.Id);
            return craft.Adapt<CraftDto>();
        }

        /// <summary>
        /// Update craft icon URL (public ID derived from URL automatically)
        /// </summary>
        public async Task<CraftDto> UpdateCraftIconAsync(int craftId, string iconUrl)
        {
            var craft = await _unitOfWork.Crafts.GetByIdAsync(craftId);

            if (craft == null)
                throw new KeyNotFoundException($"Craft with ID {craftId} not found.");

            craft.IconUrl = iconUrl;
            await _unitOfWork.Crafts.UpdateAsync(craft);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Craft icon updated successfully for ID: {CraftId}", craftId);
            return craft.Adapt<CraftDto>();
        }

        /// <summary>
        /// Delete a craft with business rule validation
        /// </summary>
        public async Task DeleteCraftAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Craft ID must be greater than zero", nameof(id));

            var craftsQuery = await _unitOfWork.Crafts.GetAllAsync();
            var craft = await craftsQuery.FirstOrDefaultAsync(c => c.Id == id);

            if (craft == null)
                throw new KeyNotFoundException($"Craft with ID {id} not found.");

            // Check if craft has active craftsmen
            var craftsmenQuery = await _unitOfWork.Craftsmen.GetAllAsync();
            var activeCraftsmen = await craftsmenQuery.AnyAsync(cr => cr.CraftId == id);

            if (activeCraftsmen)
                throw new InvalidOperationException(
                    $"Cannot delete craft '{craft.Name}' because it has associated craftsmen. " +
                    "Please reassign or remove all craftsmen first.");

            // Check if craft has active service requests
            var serviceReqQuery = await _unitOfWork.ServiceRequests.GetAllAsync();
            var activeServiceRequests = await serviceReqQuery
                .AnyAsync(sr => sr.CraftId == id && sr.Status != ServiceRequestStatus.Completed && sr.Status != ServiceRequestStatus.Cancelled && sr.Status != ServiceRequestStatus.Expired);

            if (activeServiceRequests)
                throw new InvalidOperationException(
                    $"Cannot delete craft '{craft.Name}' because it has active service requests. " +
                    "Please complete or cancel all service requests first.");

            // Perform deletion
            var craftToDelete = await _unitOfWork.Crafts.GetByIdAsync(id);
            if (craftToDelete != null)
            {
                await _unitOfWork.Crafts.DeleteAsync(craftToDelete);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Craft deleted successfully: {CraftName} with ID: {CraftId}", craft.Name, id);
            }
        }

        /// <summary>
        /// Activate or deactivate a craft
        /// </summary>
        public async Task<CraftDto> ToggleCraftActiveStatusAsync(int id, bool isActive)
        {
            if (id <= 0)
                throw new ArgumentException("Craft ID must be greater than zero", nameof(id));

            var craft = await _unitOfWork.Crafts.GetByIdAsync(id);

            if (craft == null)
                throw new KeyNotFoundException($"Craft with ID {id} not found.");

            craft.IsActive = isActive;

            await _unitOfWork.Crafts.UpdateAsync(craft);
            await _unitOfWork.SaveAsync();

            return craft.Adapt<CraftDto>();
        }

        /// <summary>
        /// Get all active crafts ordered by display order for UI display
        /// </summary>
        public async Task<IEnumerable<CraftDto>> GetActiveCraftsForDisplayAsync()
        {
            var craftsQuery = await _unitOfWork.Crafts.GetAllAsync();
            var crafts = await craftsQuery
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return await MapCraftsWithCountsAsync(crafts);
        }

        /// <summary>
        /// Check if a craft exists by ID
        /// </summary>
        public async Task<bool> CraftExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            var craftsQuery = await _unitOfWork.Crafts.GetAllAsync();
            return await craftsQuery.AnyAsync(c => c.Id == id);
        }

        /// <summary>
        /// Check if a craft name is unique
        /// </summary>
        public async Task<bool> IsCraftNameUniqueAsync(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var query = await _unitOfWork.Crafts.GetAllAsync();

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return !await query.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        // ==================== Helper Methods ====================

        /// <summary>
        /// Map a single craft to DTO with counts
        /// </summary>
        private async Task<CraftDto> MapCraftWithCountsAsync(Craft craft)
        {
            var craftsmenQuery = await _unitOfWork.Craftsmen.GetAllAsync();
            var craftsmenCount = await craftsmenQuery.CountAsync(cr => cr.CraftId == craft.Id);

            var serviceReqQuery = await _unitOfWork.ServiceRequests.GetAllAsync();
            var activeServiceRequestsCount = await serviceReqQuery
                .CountAsync(sr => sr.CraftId == craft.Id && sr.Status == ServiceRequestStatus.Open);

            var craftDto = craft.Adapt<CraftDto>();
            craftDto.CraftsmenCount = craftsmenCount;
            craftDto.ActiveServiceRequestsCount = activeServiceRequestsCount;

            return craftDto;
        }

        /// <summary>
        /// Map multiple crafts to DTOs with counts
        /// </summary>
        private async Task<IEnumerable<CraftDto>> MapCraftsWithCountsAsync(IEnumerable<Craft> crafts)
        {
            var craftDtos = new List<CraftDto>();

            foreach (var craft in crafts)
            {
                craftDtos.Add(await MapCraftWithCountsAsync(craft));
            }

            return craftDtos;
        }
    }
}
