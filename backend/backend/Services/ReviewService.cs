using backend.DTOs.Review;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repo;


        public ReviewService(IReviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<FoodRatingSummaryDto> GetByFood(int foodId)
        {
            var reviews = await _repo.GetByFoodId(foodId);
            return new FoodRatingSummaryDto
            {
                FoodId = foodId,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? Math.Round(reviews.Average(x => x.Rating), 1) : 0,
                Reviews = reviews.Select(MapToDto).ToList()
            };
        }

        public async Task<ReviewResponseDto?> GetById(int id)
        {
            var review = await _repo.GetById(id);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found.");
            }
            return MapToDto(review);
        }

        public async Task<ReviewResponseDto?> Create(int userId, CreateReviewDto dto)
        {
            var food = await _repo.GetFoodById(dto.FoodId);
            if (food == null)
            {
                throw new Exception("Food not found.");
            }
            var existing = await _repo.GetByUserAndFood(userId, dto.FoodId);
            if (existing != null)
            {
                throw new Exception("You have already reviewed this food.");
            }
            var purchased = await _repo.HasUserPurchasedFood(userId, dto.FoodId);
            if (!purchased)
            {
                throw new Exception("You can only review food you have purchased.");
            }
            var review = new Review
            {
                UserId = userId,
                FoodId = dto.FoodId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };
            var createdReview = await _repo.Create(review);
            return MapToDto(createdReview);
        }

        public async Task<ReviewResponseDto> Update(int reviewId, int currentUserId, bool isAdmin, UpdateReviewDto dto)
        {
            var review = await _repo.GetById(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found.");
            }
            if (!isAdmin && review.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not the owner of this review.");
            }
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            await _repo.Update(review);
            return MapToDto(review);
        }

        public async Task Delete(int reviewId, int currentUserId, bool isAdmin)
        {
            var review = await _repo.GetById(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found.");
            }
            if (!isAdmin && review.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not the owner of this review.");
            }
            await _repo.Delete(review);
        }

        private static ReviewResponseDto MapToDto(Review review)
        {
            return new ReviewResponseDto
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = review.User?.FullName ?? "",
                UserAvatar = review.User?.Avatar,
                FoodId = review.FoodId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
}