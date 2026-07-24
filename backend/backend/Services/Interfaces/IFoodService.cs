
using backend.DTOs.Food;
using backend.DTOs.Page;

namespace backend.Services.Interfaces
{
    public interface IFoodService
    {
        Task<PagedResultDto<FoodDto>> GetAllAsync(PaginationParams pagination);

        Task<FoodDto?> GetByIdAsync(int id);

        Task<PagedResultDto<FoodDto>> GetByCategoryAsync(int categoryId, PaginationParams pagination);
        Task<PagedResultDto<FoodDto>> GetBySystemCategoryAsync(int systemCategoryId, PaginationParams pagination);
        Task<PagedResultDto<FoodDto>> SearchAsync(string keyword, PaginationParams pagination);

        Task<FoodDto> CreateAsync(int currentUserId, bool isAdmin, CreateFoodDto dto);

        Task UpdateAsync(int id, int currentUserId, bool isAdmin, UpdateFoodDto dto);

        Task DeleteAsync(int id, int currentUserId, bool isAdmin);
    }
}
