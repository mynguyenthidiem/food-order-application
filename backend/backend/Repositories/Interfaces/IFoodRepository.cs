using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IFoodRepository
    {
        Task<IEnumerable<Food>> GetAllAsync();

        Task<Food?> GetByIdAsync(int id);

        Task<IEnumerable<Food>> GetByCategoryAsync(int categoryId);

        Task<IEnumerable<Food>> SearchAsync(string keyword);

        Task<Food> CreateAsync(Food food);

        Task UpdateAsync(Food food);

        Task DeleteAsync(Food food);

        Task<bool> ExistsAsync(int id);

        Task<bool> CategoryExistsAsync(int categoryId);
    }
}
