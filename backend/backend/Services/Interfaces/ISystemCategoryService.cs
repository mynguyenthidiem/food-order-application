using backend.DTOs.SystemCategory;

namespace backend.Services.Interfaces
{
    public interface ISystemCategoryService
    {
        Task<IEnumerable<SystemCategoryDto>> GetAllAsync();

        Task<SystemCategoryDto?> GetByIdAsync(int id);

        Task<SystemCategoryDto> CreateAsync(CreateSystemCategoryDto dto);

        Task UpdateAsync(int id, UpdateSystemCategoryDto dto);

        Task DeleteAsync(int id);
    }
}
