using backend.DTOs.Page;
using backend.DTOs.Food;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class FoodService : IFoodService
    {
        private readonly IFoodRepository _repository;
        private readonly IFileStorageService _fileStorageService;
        private IUrlService _urlService;

        public FoodService(IFoodRepository repository, IFileStorageService fileStorageService, IUrlService urlService)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _urlService = urlService;
        }

        public async Task<PagedResultDto<FoodDto>> GetAllAsync(PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.GetAllAsync(pagination.PageNumber, pagination.PageSize);
            return new PagedResultDto<FoodDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
        }

        public async Task<FoodDto?> GetByIdAsync(int id)
        {
            var food = await _repository.GetByIdAsync(id);
            if (food == null)
            {
                throw new KeyNotFoundException("Food not found.");

            }
            return MapToDto(food);
        }

        public async Task<PagedResultDto<FoodDto>> GetBySystemCategoryAsync(int systemCategoryId, PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.GetBySystemCategoryAsync(systemCategoryId, pagination.PageNumber, pagination.PageSize);
            return new PagedResultDto<FoodDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
        }

        public async Task<PagedResultDto<FoodDto>> GetByCategoryAsync(int categoryId, PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.GetByCategoryAsync(categoryId, pagination.PageNumber, pagination.PageSize);
            return new PagedResultDto<FoodDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
        }

        public async Task<PagedResultDto<FoodDto>> SearchAsync(string keyword, PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.SearchAsync(keyword, pagination.PageNumber, pagination.PageSize);
            return new PagedResultDto<FoodDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
        }

        public async Task<FoodDto> CreateAsync(int currentUserId, bool isAdmin, CreateFoodDto dto)
        {
            var category = await _repository.GetCategoryWithRestaurantAsync(dto.CategoryId);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            if (!isAdmin && category.Restaurant?.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to create food in this restaurant.");
            }

            var food = new Food
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Status = FoodStatus.Available,
                CategoryId = dto.CategoryId,
                RestaurantId = category.RestaurantId,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Image != null)
            {
                food.Image = await _fileStorageService.SaveImage(dto.Image, "foods");
            }

            await _repository.CreateAsync(food);

            food = await _repository.GetByIdAsync(food.Id);
            return MapToDto(food!);
        }

        public async Task UpdateAsync(int id, int currentUserId, bool isAdmin, UpdateFoodDto dto)
        {
            var food = await _repository.GetFoodForManagementAsync(id);

            if (food == null)
            {
                throw new KeyNotFoundException("Food not found.");
            }

            if (!isAdmin && food.Restaurant?.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to manage this food.");
            }
            // 2. Chỉ kiểm tra & đổi Category/Restaurant nếu FE gửi CategoryId mới khác với CategoryId hiện tại
            if (dto.CategoryId.HasValue && dto.CategoryId.Value != food.CategoryId)
            {
                var newCategoryId = dto.CategoryId.Value;
                var newCategory = await _repository.GetCategoryWithRestaurantAsync(newCategoryId);

                if (newCategory == null)
                {
                    throw new KeyNotFoundException("New category not found.");
                }

                if (!isAdmin && newCategory.Restaurant?.OwnerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("Cannot move food to another owner's category.");
                }

                food.CategoryId = newCategoryId;
                food.RestaurantId = newCategory.RestaurantId;
            }

            if (!IsValidStatus(dto.Status))
            {
                throw new ArgumentException("Invalid status.");
            }

            food.Name = dto.Name;
            food.Description = dto.Description;
            food.Price = dto.Price;
            food.Status = dto.Status;

            if (dto.Image != null)
            {
                if (!string.IsNullOrEmpty(food.Image))
                {
                    await _fileStorageService.DeleteImage(food.Image);
                }

                food.Image = await _fileStorageService.SaveImage(dto.Image, "foods");
            }

            await _repository.UpdateAsync(food);
        }

        public async Task DeleteAsync(int id, int currentUserId, bool isAdmin)
        {
            var food = await _repository.GetFoodForManagementAsync(id);
            if (food == null)
            {
                throw new KeyNotFoundException("Food not found.");
            }

            if (!isAdmin && food.Restaurant?.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to manage this food.");
            }

            food.Status = FoodStatus.Unavailable;
            await _repository.UpdateAsync(food);
        }

        private FoodDto MapToDto(Food food)
        {
            return new FoodDto
            {
                Id = food.Id,
                Name = food.Name,
                Description = food.Description,
                Price = food.Price,
                Image = string.IsNullOrEmpty(food.Image) ? null : _urlService.GetAbsoluteUrl(food.Image),
                Status = food.Status,
                CategoryId = food.CategoryId,
                CategoryName = food.Category?.SystemCategory?.Name,
                CreatedAt = food.CreatedAt
            };
        }

        private bool IsValidStatus(FoodStatus status)
        {
            return Enum.IsDefined(typeof(FoodStatus), status);
        }
    }
}