using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IFoodRepository
    {
        Task<(List<Food> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);

        Task<Food?> GetByIdAsync(int id);
        Task<Food?> GetFoodForManagementAsync(int id);

        Task<(List<Food> Items,  int TotalCount)> GetByCategoryAsync(int categoryId, int pageNumber, int pageSize);

        Task<(List<Food> Items, int TotalCount)> GetBySystemCategoryAsync(int systemCategoryId, int pageNumber, int PageSize);
        Task<(List<Food> Items, int TotalCount)> SearchAsync(string keyword, int pageNumber, int pageSize);

        Task CreateAsync(Food food);

        Task UpdateAsync(Food food);

        Task DeleteAsync(Food food);

        Task<bool> ExistsAsync(int id);

        Task<bool> CategoryExistsAsync(int categoryId);
        Task<Category?> GetCategoryWithRestaurantAsync(int categoryId);
    }
}
