using backend.DTOs.Food;

namespace backend.Services.Interfaces
{
    public interface IFoodService
    {
        Task<IEnumerable<FoodDto>> GetAllAsync();

        Task<FoodDto?> GetByIdAsync(int id);

        Task<IEnumerable<FoodDto>> GetByCategoryAsync(int categoryId);

        Task<IEnumerable<FoodDto>> SearchAsync(string keyword);

        Task<FoodDto> CreateAsync(int currentUserId,bool isAdmin,CreateFoodDto dto);

        Task UpdateAsync(int id,int currentUserId,bool isAdmin, UpdateFoodDto dto);

        Task DeleteAsync(int id, int currentUserId, bool isAdmin);
    }
}
