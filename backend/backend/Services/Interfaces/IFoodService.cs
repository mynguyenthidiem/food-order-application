using backend.DTOs.Food;

namespace backend.Services.Interfaces
{
    public interface IFoodService
    {
        Task<IEnumerable<FoodDto>> GetAllAsync();

        Task<FoodDto?> GetByIdAsync(int id);

        Task<IEnumerable<FoodDto>> GetByCategoryAsync(int categoryId);

        Task<IEnumerable<FoodDto>> SearchAsync(string keyword);

        Task<FoodDto> CreateAsync(CreateFoodDto dto);

        Task<bool> UpdateAsync(int id, UpdateFoodDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
