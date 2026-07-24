using backend.DTOs.Page;
using backend.DTOs.Review;

namespace backend.Services.Interfaces
{
    public interface IReviewService
    {
        Task<FoodRatingSummaryDto> GetByFood(int foodId, PaginationParams pagination);
        Task<ReviewResponseDto?> GetById(int id);
        Task<ReviewResponseDto?> Create(int userId, CreateReviewDto dto);
        Task<ReviewResponseDto> Update(int reviewId, int currentUserId, bool isAdmin, UpdateReviewDto dto);
        Task Delete(int reviewId, int currentUserId, bool isAdmin);
    }

}