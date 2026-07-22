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

        public FoodService(IFoodRepository repository, IFileStorageService fileStorageService)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
        }

        public async Task<IEnumerable<FoodDto>> GetAllAsync()
        {
            var foods = await _repository.GetAllAsync();
            return foods.Select(MapToDto);
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

        public async Task<IEnumerable<FoodDto>> GetByCategoryAsync(int categoryId)
        {
            var foods = await _repository.GetByCategoryAsync(categoryId);
            return foods.Select(MapToDto);
        }

        public async Task<IEnumerable<FoodDto>> SearchAsync(string keyword)
        {
            var foods = await _repository.SearchAsync(keyword);
            return foods.Select(MapToDto);
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
            var food = await _repository.GetByIdAsync(id);

            if (food == null)
            {
                throw new KeyNotFoundException("Food not found.");
            }

            // 1. Kiểm tra quyền sở hữu đối với nhà hàng hiện tại chứa món ăn
            var category = await _repository.GetCategoryWithRestaurantAsync(food.CategoryId);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            if (!isAdmin && category.Restaurant?.OwnerId != currentUserId)
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
            var food = await _repository.GetByIdAsync(id);
            if (food == null)
            {
                throw new KeyNotFoundException("Food not found.");
            }

            var category = await _repository.GetCategoryWithRestaurantAsync(food.CategoryId);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            if (!isAdmin && category.Restaurant?.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to manage this food.");
            }

            food.Status = FoodStatus.Unavailable;
            await _repository.UpdateAsync(food);
        }

        private static FoodDto MapToDto(Food food)
        {
            return new FoodDto
            {
                Id = food.Id,
                Name = food.Name,
                Description = food.Description,
                Price = food.Price,
                Image = food.Image,
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