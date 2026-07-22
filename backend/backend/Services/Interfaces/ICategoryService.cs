using backend.DTOs.Category;

namespace backend.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();

        Task<CategoryDto?> GetByIdAsync(int id);

        Task<CategoryDto> CreateAsync(int currentUserId, bool isAdmin, CreateCategoryDto dto);

        Task UpdateAsync(int id, int currentUserId, bool isAdmin, UpdateCategoryDto dto);

        Task DeleteAsync(int id, int currentUserId, bool isAdmin);
    }
}
