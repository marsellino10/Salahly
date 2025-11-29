using Mapster;
using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salahly.DSL.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CountServiceRequestsAsync(DateTime? from = null, DateTime? to = null, int? craftId = null, int? areaId = null)
        {
            var q = _unitOfWork.ServiceRequests.GetAll();
            if (from.HasValue) q = q.Where(sr => sr.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(sr => sr.CreatedAt <= to.Value);
            if (craftId.HasValue) q = q.Where(sr => sr.CraftId == craftId.Value);
            if (areaId.HasValue) q = q.Where(sr => sr.AreaId == areaId.Value);
            return await q.CountAsync();
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsFilteredAsync(DateTime? from = null, DateTime? to = null, int? craftId = null, int? areaId = null, string orderBy = "date", bool asc = false)
        {
            var q = _unitOfWork.ServiceRequests.GetAll()
                .Include(sr => sr.Craft)
                .Include(sr => sr.AreaData)
                .Include(sr => sr.Customer).ThenInclude(c => c.User)
                .AsQueryable();

            if (from.HasValue) q = q.Where(sr => sr.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(sr => sr.CreatedAt <= to.Value);
            if (craftId.HasValue) q = q.Where(sr => sr.CraftId == craftId.Value);
            if (areaId.HasValue) q = q.Where(sr => sr.AreaId == areaId.Value);

            q = orderBy.ToLower() switch
            {
                "craft" => asc ? q.OrderBy(sr => sr.Craft.Name) : q.OrderByDescending(sr => sr.Craft.Name),
                "area" => asc ? q.OrderBy(sr => sr.AreaData.City).ThenBy(sr => sr.AreaData.Region) : q.OrderByDescending(sr => sr.AreaData.City).ThenByDescending(sr => sr.AreaData.Region),
                _ => asc ? q.OrderBy(sr => sr.CreatedAt) : q.OrderByDescending(sr => sr.CreatedAt)
            };

            var list = await q.ToListAsync();
            return list.Select(sr => sr.Adapt<ServiceRequestDto>());
        }

        public async Task<AreaStatsDto?> GetMostActiveAreaAsync()
        {
            var q = _unitOfWork.ServiceRequests.GetAll()
                .Include(sr => sr.AreaData)
                .AsQueryable();

            var grouped = await q
                .GroupBy(sr => new { sr.AreaId, sr.AreaData.Region, sr.AreaData.City })
                .Select(g => new AreaStatsDto
                {
                    AreaId = g.Key.AreaId,
                    Region = g.Key.Region,
                    City = g.Key.City,
                    RequestCount = g.Count()
                })
                .OrderByDescending(a => a.RequestCount)
                .FirstOrDefaultAsync();

            return grouped;
        }

        public async Task<OffersStatsDto> GetOffersStatsAsync()
        {
            var offers = _unitOfWork.CraftsmanOffers.GetAll();
            var totalOffers = await offers.CountAsync();
            var totalServiceRequests = await _unitOfWork.ServiceRequests.GetAll().CountAsync();
            var avg = totalServiceRequests == 0 ? 0 : (double)totalOffers / totalServiceRequests;
            return new OffersStatsDto { TotalOffers = totalOffers, AverageOffersPerServiceRequest = avg };
        }

        public async Task<int> CountCraftsmenAsync(int? craftId = null, int? areaId = null)
        {
            var q = _unitOfWork.Craftsmen.GetAll();
            if (craftId.HasValue) q = q.Where(c => c.CraftId == craftId.Value);
            if (areaId.HasValue)
            {
                q = q.Where(c => c.CraftsmanServiceAreas.Any(sa => sa.AreaId == areaId.Value));
            }
            return await q.CountAsync();
        }

        public async Task<int> GetTotalCraftsmenExperienceAsync(int? craftId = null, int? areaId = null)
        {
            var q = _unitOfWork.Craftsmen.GetAll();
            if (craftId.HasValue) q = q.Where(c => c.CraftId == craftId.Value);
            if (areaId.HasValue)
            {
                q = q.Where(c => c.CraftsmanServiceAreas.Any(sa => sa.AreaId == areaId.Value));
            }
            var sum = await q.Select(c => c.YearsOfExperience).SumAsync();
            return sum;
        }

        public async Task<IEnumerable<CraftsmanShortDto>> GetTopCraftsmenByReviewsAsync(int top = 5)
        {
            var q = _unitOfWork.Craftsmen.GetAll()
                .Include(c => c.User)
                    .ThenInclude(u => u.ReviewsReceived)
                .Include(c => c.Craft)
                .AsQueryable();

            var list = await q
                .OrderByDescending(c => c.User.RatingAverage)
                .Take(top)
                .ToListAsync();

            return list.Select(c => c.Adapt<CraftsmanShortDto>());
        }

        public async Task<int> CountCraftsAsync()
        {
            return await _unitOfWork.Crafts.GetAll().CountAsync();
        }

        public async Task<IEnumerable<CraftAverageReviewDto>> GetCraftsAverageReviewsAsync()
        {
            var q = _unitOfWork.Crafts.GetAll()
                .Include(c => c.Craftsmen)
                    .ThenInclude(cr => cr.User)
                .AsQueryable();

            var result = await q.Select(c => new CraftAverageReviewDto
            {
                CraftId = c.Id,
                CraftName = c.Name,

                AverageReview = c.Craftsmen
             .Average(cm => (float?)cm.User.RatingAverage) ?? 0
            }).ToListAsync();

            return result;
        }

        public async Task<bool> ApprovePortfolioItemAsync(int portfolioItemId)
        {
            var item = await _unitOfWork.PortfolioItems.GetByIdAsync(portfolioItemId);
            if (item == null) return false;
            item.IsActive = true;
            await _unitOfWork.PortfolioItems.UpdateAsync(item);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
