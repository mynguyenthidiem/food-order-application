using backend.DTOs.Category;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IUrlService _urlService;

        public CategoryService(ICategoryRepository repository, IUrlService urlService)
        {
            _repository = repository;
            _urlService = urlService;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }
            return MapToDto(category);
        }

        public async Task<CategoryDto> CreateAsync(int currentUserId, bool isAdmin, CreateCategoryDto dto)
        {
            var restaurant = await _repository.GetRestaurantAsync(dto.RestaurantId);

            if (restaurant == null)
                throw new KeyNotFoundException("Restaurant not found.");

            if (!isAdmin &&
                restaurant.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to manage this restaurant.");
            }

            var systemCategory = await _repository.GetSystemCategoryAsync(dto.SystemCategoryId);
            if (systemCategory == null)
            {
                throw new KeyNotFoundException("System category not found.");
            }

            if (await _repository.ExistsByRestaurantAndSystemCategoryAsync(dto.RestaurantId, dto.SystemCategoryId))
            {
                throw new ArgumentException("This restaurant has already added this category.");
            }

            var category = new Category
            {
                RestaurantId = dto.RestaurantId,
                SystemCategoryId = dto.SystemCategoryId
            };

            await _repository.CreateAsync(category);

            // Nạp lại SystemCategory để map DTO trả về đầy đủ Name/Description/Image
            category.SystemCategory = systemCategory;
            return MapToDto(category);
        }

        public async Task UpdateAsync(int id, int currentUserId, bool isAdmin, UpdateCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }
            if (category.Restaurant == null)
            {
                throw new KeyNotFoundException("Restaurant not found.");
            }

            if (!isAdmin && category.Restaurant.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to manage this category.");
            }

            var systemCategory = await _repository.GetSystemCategoryAsync(dto.SystemCategoryId);
            if (systemCategory == null)
            {
                throw new KeyNotFoundException("System category not found.");
            }

            if (await _repository.ExistsByRestaurantAndSystemCategoryAsync(category.RestaurantId, dto.SystemCategoryId, category.Id))
            {
                throw new ArgumentException("This restaurant has already added this category.");
            }

            category.SystemCategoryId = dto.SystemCategoryId;
            await _repository.UpdateAsync(category);
        }

        public async Task DeleteAsync(int id, int currentUserId, bool isAdmin)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            if (category.Restaurant == null)
            {
                throw new KeyNotFoundException("Restaurant not found.");
            }

            if (!isAdmin && category.Restaurant.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to manage this category.");
            }

            category.IsActive = false;
            await _repository.UpdateAsync(category);
            await _repository.DeactivateFoodsByCategoryAsync(category.Id);
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                RestaurantId = category.RestaurantId,
                SystemCategoryId = category.SystemCategoryId,
                Name = category.SystemCategory?.Name ?? string.Empty,
                Description = category.SystemCategory?.Description,
                Image = string.IsNullOrEmpty(category.SystemCategory?.Image) ? null : _urlService.GetAbsoluteUrl(category.SystemCategory.Image)
            };
        }
    }
}
