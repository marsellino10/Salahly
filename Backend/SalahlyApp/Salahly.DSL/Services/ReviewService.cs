using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;

namespace Salahly.DSL.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CraftsmanReviewDto>> GetReviewsForCraftsmanAsync(int craftsmanId)
        {
            if (craftsmanId <= 0)
            {
                return Enumerable.Empty<CraftsmanReviewDto>();
            }

            var craftsmanUser = await _unitOfWork.ApplicationUsers.GetByIdAsync(craftsmanId);
            if (craftsmanUser == null)
            {
                return Enumerable.Empty<CraftsmanReviewDto>();
            }

            var reviews = await _unitOfWork.Reviews.GetReviewsByUserIdAsync(craftsmanUser.Id);
            return reviews.Adapt<IEnumerable<CraftsmanReviewDto>>();
        }
        public async Task<bool> CreateReviewAsync(CreateReviewDto dto)
        {
            var check = await HasUserReviewedAsync(dto.ReviewerUserId, dto.BookingId);
            if (check) return false;
            var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId);
            if (booking.Status != BookingStatus.Completed)
            {
                return false;
            }
            var review = new Review
            {
                BookingId=dto.BookingId,
                Comment=dto.Comment,
                Rating=dto.Rating,
                ReviewerUserId=dto.ReviewerUserId,
                TargetUserId=dto.TargetUserId
            };
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveAsync();
            var avg = await GetAverageRatingForUser(dto.TargetUserId);
            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(dto.TargetUserId);
            user.RatingAverage = avg;
            await _unitOfWork.ApplicationUsers.UpdateAsync(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int requestingUserId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null) return false;
            if (review.ReviewerUserId != requestingUserId) return false;
            await _unitOfWork.Reviews.DeleteAsync(review);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<double> GetAverageRatingForUser(int userId)
        {
            return await _unitOfWork.Reviews.GetAverageRatingForUser(userId);
        }

        public async Task<IEnumerable<CreateReviewDto>> GetReviewsForBookingAsync(int BookingId)
        {
            var reviews= await _unitOfWork.Reviews.GetReviewsByBookingIdAsync(BookingId);
            return  reviews.Adapt<IEnumerable<CreateReviewDto>>();
        }

        public async Task<IEnumerable<CreateReviewDto>> GetReviewsForUserAsync(int userId)
        {
            var reviews = await _unitOfWork.Reviews.GetReviewsByUserIdAsync(userId);
            return reviews.Adapt<IEnumerable<CreateReviewDto>>();
        }

        public Task<bool> HasUserReviewedAsync(int reviewerId, int BookingId)
        {
            return _unitOfWork.Reviews.HasUserReviewedAsync(reviewerId, BookingId);
        }
    }
}
