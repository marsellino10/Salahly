using Salahly.DSL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Interfaces
{
    /// <summary>
    /// Service interface for Craft business logic operations
    /// Acts as a bridge between the controller and data access layer
    /// </summary>
    public interface ICraftService
    {
        /// <summary>
        /// Retrieve all crafts with optional filtering
        /// </summary>
        /// <param name="isActiveOnly">Filter to show only active crafts</param>
        /// <returns> list of crafts</returns>
        Task<IEnumerable<CraftDto>> GetAllCraftsAsync(bool isActiveOnly = false);

        /// <summary>
        /// Retrieve a single craft by ID with additional details
        /// </summary>
        /// <param name="id">Craft ID</param>
        /// <returns>Craft details or null if not found</returns>
        Task<CraftDto?> GetCraftByIdAsync(int id);

        /// <summary>
        /// Retrieve a craft by name
        /// </summary>
        /// <param name="name">Craft name</param>
        /// <returns>Craft details or null if not found</returns>
        Task<CraftDto?> GetCraftByNameAsync(string name);

        /// <summary>
        /// Create a new craft
        /// </summary>
        /// <param name="createCraftDto">DTO containing craft creation data</param>
        /// <returns>Created craft details</returns>
        /// <exception cref="InvalidOperationException">Thrown when craft name already exists</exception>
        Task<CraftDto> CreateCraftAsync(CreateCraftDto createCraftDto);

        /// <summary>
        /// Update an existing craft
        /// </summary>
        /// <param name="updateCraftDto">DTO containing updated craft data</param>
        /// <returns>Updated craft details</returns>
        /// <exception cref="KeyNotFoundException">Thrown when craft is not found</exception>
        /// <exception cref="InvalidOperationException">Thrown when craft name already exists for another craft</exception>
        Task<CraftDto> UpdateCraftAsync(UpdateCraftDto updateCraftDto);

        /// <summary>
        /// Update craft icon URL (public ID derived from URL automatically)
        /// </summary>
        /// <param name="craftId">Craft ID</param>
        /// <param name="iconUrl">New icon URL from Cloudinary</param>
        /// <returns>Updated craft details</returns>
        Task<CraftDto> UpdateCraftIconAsync(int craftId, string iconUrl);

        /// <summary>
        /// Delete a craft by ID
        /// </summary>
        /// <param name="id">Craft ID</param>
        /// <exception cref="KeyNotFoundException">Thrown when craft is not found</exception>
        /// <exception cref="InvalidOperationException">Thrown when craft has active craftsmen or service requests</exception>
        Task DeleteCraftAsync(int id);

        /// <summary>
        /// Activate or deactivate a craft
        /// </summary>
        /// <param name="id">Craft ID</param>
        /// <param name="isActive">True to activate, false to deactivate</param>
        /// <returns>Updated craft details</returns>
        /// <exception cref="KeyNotFoundException">Thrown when craft is not found</exception>
        Task<CraftDto> ToggleCraftActiveStatusAsync(int id, bool isActive);

        /// <summary>
        /// Get all active crafts ordered by display order (for UI/frontend)
        /// </summary>
        /// <returns>List of active crafts</returns>
        Task<IEnumerable<CraftDto>> GetActiveCraftsForDisplayAsync();

        /// <summary>
        /// Check if a craft exists by ID
        /// </summary>
        /// <param name="id">Craft ID</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> CraftExistsAsync(int id);

        /// <summary>
        /// Check if a craft name is unique
        /// </summary>
        /// <param name="name">Craft name to check</param>
        /// <param name="excludeId">Optional craft ID to exclude from check (for updates)</param>
        /// <returns>True if name is unique, false otherwise</returns>
        Task<bool> IsCraftNameUniqueAsync(string name, int? excludeId = null);
    }
}
