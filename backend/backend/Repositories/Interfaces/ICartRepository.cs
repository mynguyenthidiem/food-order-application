using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<IEnumerable<Cart>> GetUserCartAsync(int userId);

        Task<Cart?> GetByIdAsync(int id);

        Task<Cart?> GetByUserAndFoodAsync(int userId, int foodId);

        Task<bool> FoodExistsAsync(int foodId);

        Task<Cart> AddAsync(Cart cart);

        Task<Food?> GetFoodByIdAsync(int foodId);

        Task UpdateAsync(Cart cart);

        Task DeleteAsync(Cart cart);

        Task ClearAsync(int userId);
    }
}