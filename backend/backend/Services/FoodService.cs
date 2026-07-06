using backend.DTOs.Food;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class FoodService : IFoodService
    {
        private readonly IFoodRepository _repository;

        public FoodService(IFoodRepository repository)
        {
            _repository = repository;
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
                return null;

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

        public async Task<FoodDto> CreateAsync(CreateFoodDto dto)
        {
            if (!await _repository.CategoryExistsAsync(dto.CategoryId))
                throw new Exception("Category not found.");

            if (!IsValidStatus(dto.Status))
                throw new Exception("Invalid status.");

            var food = new Food
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Image = dto.Image,
                Status = dto.Status,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(food);

            food = await _repository.GetByIdAsync(food.Id);

            return MapToDto(food!);
        }

        public async Task<bool> UpdateAsync(int id, UpdateFoodDto dto)
        {
            var food = await _repository.GetByIdAsync(id);

            if (food == null)
                return false;

            if (!await _repository.CategoryExistsAsync(dto.CategoryId))
                throw new Exception("Category not found.");

            if (!IsValidStatus(dto.Status))
                throw new Exception("Invalid status.");

            food.Name = dto.Name;
            food.Description = dto.Description;
            food.Price = dto.Price;
            food.Image = dto.Image;
            food.Status = dto.Status;
            food.CategoryId = dto.CategoryId;

            await _repository.UpdateAsync(food);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var food = await _repository.GetByIdAsync(id);

            if (food == null)
                return false;

            await _repository.DeleteAsync(food);

            return true;
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
                CategoryName = food.Category?.Name,
                CreatedAt = food.CreatedAt
            };
        }

        private static bool IsValidStatus(string status)
        {
            return status == "Available"
                || status == "OutOfStock"
                || status == "Hidden";
        }
    }
}
