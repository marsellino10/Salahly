using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs.PortfolioDtos;
using Salahly.DSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salahly.DSL.Services
{
    /// <summary>
    /// Service for managing craftsman portfolio items
    /// Handles CRUD operations for portfolio items using UnitOfWork pattern
    /// </summary>
    public class PortfolioService : IPortfolioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(IUnitOfWork unitOfWork, ILogger<PortfolioService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all portfolio items for a specific craftsman
        /// </summary>
        public async Task<IEnumerable<PortfolioItemResponseDto>> GetByCraftsmanAsync(int craftsmanId)
        {
            if (craftsmanId <= 0)
            {
                _logger.LogWarning("Invalid craftsman ID: {CraftsmanId}", craftsmanId);
                return Enumerable.Empty<PortfolioItemResponseDto>();
            }

            try
            {
                // Verify craftsman exists
                var craftsman = await _unitOfWork.Craftsmen.GetByIdAsync(craftsmanId);
                if (craftsman == null)
                {
                    _logger.LogWarning("Craftsman not found: {CraftsmanId}", craftsmanId);
                    return Enumerable.Empty<PortfolioItemResponseDto>();
                }

                var items = (await _unitOfWork.PortfolioItems.GetAllAsync())
                    .Where(p => p.CraftsmanId == craftsmanId)
                    .OrderBy(p => p.DisplayOrder)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} portfolio items for craftsman {CraftsmanId}", items.Count, craftsmanId);
                return items.Adapt<IEnumerable<PortfolioItemResponseDto>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio items for craftsman {CraftsmanId}", craftsmanId);
                throw;
            }
        }

        /// <summary>
        /// Get a single portfolio item by ID
        /// </summary>
        public async Task<PortfolioItemResponseDto?> GetByIdAsync(int portfolioItemId)
        {
            if (portfolioItemId <= 0)
            {
                _logger.LogWarning("Invalid portfolio item ID: {PortfolioItemId}", portfolioItemId);
                return null;
            }

            try
            {
                var item = await _unitOfWork.PortfolioItems.GetByIdAsync(portfolioItemId);
                if (item == null)
                {
                    _logger.LogWarning("Portfolio item not found: {PortfolioItemId}", portfolioItemId);
                    return null;
                }

                return item.Adapt<PortfolioItemResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio item {PortfolioItemId}", portfolioItemId);
                throw;
            }
        }

        /// <summary>
        /// Create a new portfolio item
        /// </summary>
        public async Task<PortfolioItemResponseDto> CreateAsync(CreatePortfolioItemDto dto, string imageUrl)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL is required", nameof(imageUrl));

            if (dto.CraftsmanId <= 0)
                throw new ArgumentException("Invalid craftsman ID", nameof(dto.CraftsmanId));

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Portfolio title is required", nameof(dto.Title));

            try
            {
                // Verify craftsman exists
                var craftsman = await _unitOfWork.Craftsmen.GetByIdAsync(dto.CraftsmanId);
                if (craftsman == null)
                    throw new KeyNotFoundException($"Craftsman with ID {dto.CraftsmanId} not found");

                var portfolioItem = new PortfolioItem
                {
                    CraftsmanId = dto.CraftsmanId,
                    Title = dto.Title,
                    Description = dto.Description,
                    ImageUrl = imageUrl,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.PortfolioItems.AddAsync(portfolioItem);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Portfolio item created successfully with ID: {PortfolioItemId} for craftsman {CraftsmanId}",
                    portfolioItem.Id, dto.CraftsmanId);

                return portfolioItem.Adapt<PortfolioItemResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portfolio item for craftsman {CraftsmanId}", dto.CraftsmanId);
                throw;
            }
        }

        /// <summary>
        /// Update an existing portfolio item
        /// </summary>
        public async Task<PortfolioItemResponseDto> UpdateAsync(UpdatePortfolioItemDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Id <= 0)
                throw new ArgumentException("Invalid portfolio item ID", nameof(dto.Id));

            try
            {
                var item = await _unitOfWork.PortfolioItems.GetByIdAsync(dto.Id);
                if (item == null)
                    throw new KeyNotFoundException($"Portfolio item with ID {dto.Id} not found");

                // Update properties
                item.Title = dto.Title ?? item.Title;
                item.Description = dto.Description ?? item.Description;
                if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                    item.ImageUrl = dto.ImageUrl;
                item.DisplayOrder = dto.DisplayOrder;
                item.IsActive = dto.IsActive;

                await _unitOfWork.PortfolioItems.UpdateAsync(item);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Portfolio item updated successfully: {PortfolioItemId}", dto.Id);

                return item.Adapt<PortfolioItemResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating portfolio item {PortfolioItemId}", dto.Id);
                throw;
            }
        }

        /// <summary>
        /// Delete a portfolio item by ID
        /// Returns the image URL of the deleted item
        /// </summary>
        public async Task<string?> DeleteAsync(int portfolioItemId)
        {
            if (portfolioItemId <= 0)
            {
                _logger.LogWarning("Invalid portfolio item ID: {PortfolioItemId}", portfolioItemId);
                return null;
            }

            try
            {
                var item = await _unitOfWork.PortfolioItems.GetByIdAsync(portfolioItemId);
                if (item == null)
                {
                    _logger.LogWarning("Portfolio item not found: {PortfolioItemId}", portfolioItemId);
                    return null;
                }

                var imageUrl = item.ImageUrl;

                await _unitOfWork.PortfolioItems.DeleteAsync(item);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Portfolio item deleted successfully: {PortfolioItemId}", portfolioItemId);

                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio item {PortfolioItemId}", portfolioItemId);
                throw;
            }
        }

        /// <summary>
        /// Get portfolio items count for a craftsman
        /// </summary>
        public async Task<int> GetCraftsmanPortfolioCountAsync(int craftsmanId)
        {
            if (craftsmanId <= 0)
                return 0;

            try
            {
                var items = (await _unitOfWork.PortfolioItems.GetAllAsync())
                    .Where(p => p.CraftsmanId == craftsmanId)
                    .Count();

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio count for craftsman {CraftsmanId}", craftsmanId);
                return 0;
            }
        }

        /// <summary>
        /// Get active portfolio items for a craftsman
        /// </summary>
        public async Task<IEnumerable<PortfolioItemResponseDto>> GetActiveByCraftsmanAsync(int craftsmanId)
        {
            if (craftsmanId <= 0)
            {
                _logger.LogWarning("Invalid craftsman ID: {CraftsmanId}", craftsmanId);
                return Enumerable.Empty<PortfolioItemResponseDto>();
            }

            try
            {
                var items = (await _unitOfWork.PortfolioItems.GetAllAsync())
                    .Where(p => p.CraftsmanId == craftsmanId && p.IsActive)
                    .OrderBy(p => p.DisplayOrder)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} active portfolio items for craftsman {CraftsmanId}",
                    items.Count, craftsmanId);
                return items.Adapt<IEnumerable<PortfolioItemResponseDto>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active portfolio items for craftsman {CraftsmanId}", craftsmanId);
                throw;
            }
        }

        /// <summary>
        /// Toggle portfolio item active status
        /// </summary>
        public async Task<PortfolioItemResponseDto> ToggleActiveStatusAsync(int portfolioItemId)
        {
            if (portfolioItemId <= 0)
                throw new ArgumentException("Invalid portfolio item ID", nameof(portfolioItemId));

            try
            {
                var item = await _unitOfWork.PortfolioItems.GetByIdAsync(portfolioItemId);
                if (item == null)
                    throw new KeyNotFoundException($"Portfolio item with ID {portfolioItemId} not found");

                item.IsActive = !item.IsActive;

                await _unitOfWork.PortfolioItems.UpdateAsync(item);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Portfolio item status toggled: {PortfolioItemId}, IsActive: {IsActive}",
                    portfolioItemId, item.IsActive);

                return item.Adapt<PortfolioItemResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling portfolio item status {PortfolioItemId}", portfolioItemId);
                throw;
            }
        }

        /// <summary>
        /// Reorder portfolio items by display order
        /// </summary>
        public async Task<IEnumerable<PortfolioItemResponseDto>> ReorderItemsAsync(int craftsmanId, Dictionary<int, int> itemIdDisplayOrderMap)
        {
            if (craftsmanId <= 0)
                throw new ArgumentException("Invalid craftsman ID", nameof(craftsmanId));

            if (itemIdDisplayOrderMap == null || itemIdDisplayOrderMap.Count == 0)
                throw new ArgumentException("Item display order map cannot be empty", nameof(itemIdDisplayOrderMap));

            try
            {
                // Verify craftsman exists
                var craftsman = await _unitOfWork.Craftsmen.GetByIdAsync(craftsmanId);
                if (craftsman == null)
                    throw new KeyNotFoundException($"Craftsman with ID {craftsmanId} not found");

                var items = (await _unitOfWork.PortfolioItems.GetAllAsync())
                    .Where(p => p.CraftsmanId == craftsmanId)
                    .ToList();

                // Update display orders
                foreach (var item in items)
                {
                    if (itemIdDisplayOrderMap.TryGetValue(item.Id, out var newOrder))
                    {
                        item.DisplayOrder = newOrder;
                        await _unitOfWork.PortfolioItems.UpdateAsync(item);
                    }
                }

                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Portfolio items reordered for craftsman {CraftsmanId}", craftsmanId);

                var updatedItems = (await _unitOfWork.PortfolioItems.GetAllAsync())
                    .Where(p => p.CraftsmanId == craftsmanId)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList();

                return updatedItems.Adapt<IEnumerable<PortfolioItemResponseDto>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering portfolio items for craftsman {CraftsmanId}", craftsmanId);
                throw;
            }
        }
    }
}
