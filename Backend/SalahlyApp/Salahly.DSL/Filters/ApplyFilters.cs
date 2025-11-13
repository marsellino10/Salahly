using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Entities;

namespace Salahly.DSL.Filters
{
    /// <summary>
    /// Helper class to apply filters and pagination to craftsman queries
    /// </summary>
    public static class ApplyFilters
    {
        /// <summary>
        /// Apply filters to a craftsman query and return paginated results
        /// </summary>
        /// <param name="query">The base IQueryable<Craftsman> query</param>
        /// <param name="filter">Filter parameters</param>
        /// <returns>Paginated response with filtered craftsmen</returns>
        public static async Task<PaginatedResponse<Craftsman>> ApplyAsync(
            IQueryable<Craftsman> query,
            CraftsmanFilterDto filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            // Validate filter parameters
            filter.Validate();

            // Apply search filter by name
            if (!string.IsNullOrWhiteSpace(filter.SearchName))
            {
                var searchTerm = filter.SearchName.Trim().ToLower();
                query = query.Where(c => c.User != null && 
                    c.User.FullName.ToLower().Contains(searchTerm));
            }

            // Apply craft filter
            if (filter.CraftId.HasValue && filter.CraftId.Value > 0)
            {
                query = query.Where(c => c.CraftId == filter.CraftId.Value);
            }

            // Apply area filter
            if (filter.AreaId.HasValue && filter.AreaId.Value > 0)
            {
                query = query.Where(c => c.CraftsmanServiceAreas.Any(sa => sa.AreaId == filter.AreaId.Value && sa.IsActive));
            }

            // Apply availability filter
            if (filter.IsAvailable.HasValue)
            {
                query = query.Where(c => c.IsAvailable == filter.IsAvailable.Value);
            }

            // Apply minimum rating filter
            if (filter.MinRating.HasValue && filter.MinRating.Value > 0)
            {
                query = query.Where(c => c.RatingAverage >= filter.MinRating.Value);
            }

            // Apply maximum hourly rate filter
            if (filter.MaxHourlyRate.HasValue && filter.MaxHourlyRate.Value > 0)
            {
                query = query.Where(c => c.HourlyRate.HasValue && c.HourlyRate.Value <= filter.MaxHourlyRate.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var skip = (filter.PageNumber - 1) * filter.PageSize;
            var items = await query
                .OrderByDescending(c => c.RatingAverage)
                .ThenByDescending(c => c.TotalCompletedBookings)
                .Skip(skip)
                .Take(filter.PageSize)
                .Include(c => c.User)
                .Include(c => c.Craft)
                .Include(c => c.Portfolio)
                .Include(c => c.CraftsmanServiceAreas)
                    .ThenInclude(sa => sa.Area)
                .ToListAsync();

            return new PaginatedResponse<Craftsman>(items, totalCount, filter.PageNumber, filter.PageSize);
        }
    }
}
