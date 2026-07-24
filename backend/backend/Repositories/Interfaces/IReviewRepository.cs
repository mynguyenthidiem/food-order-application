using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetByFoodId(int foodId);

        Task<(List<Review> Items, int TotalCount)> GetByFoodIdPaged(int foodId, int pageNumber, int pageSize);

        Task<Review?> GetById(int id);

        Task<Food?> GetFoodById(int foodId);

        Task<Review?> GetByUserAndFood(int userId, int foodId);

        Task<bool> HasUserPurchasedFood(int userId, int foodId);

        Task<Review> Create(Review review);

        Task Update(Review review);

        Task Delete(Review review);
    }
}