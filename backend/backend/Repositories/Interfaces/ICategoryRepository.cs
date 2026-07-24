using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(int id);

        Task<Category> CreateAsync(Category category);

        Task UpdateAsync(Category category);

        Task DeleteAsync(Category category);

        Task<bool> ExistsAsync(int id);

        Task<Category?> GetWithRestaurantAsync(int id);

        Task<Restaurant?> GetRestaurantAsync(int restaurantId);

        Task<SystemCategory?> GetSystemCategoryAsync(int systemCategoryId);

        Task<bool> ExistsByRestaurantAndSystemCategoryAsync(int restaurantId, int systemCategoryId, int? excludeCategoryId = null);
        Task DeactivateFoodsByCategoryAsync(int categoryId);
    }
}
