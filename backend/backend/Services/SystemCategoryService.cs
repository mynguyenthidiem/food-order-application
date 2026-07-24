using backend.DTOs.SystemCategory;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class SystemCategoryService : ISystemCategoryService
    {
        private readonly ISystemCategoryRepository _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUrlService _urlService;
        public SystemCategoryService(ISystemCategoryRepository repository, IFileStorageService fileStorageService, IUrlService urlService)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _urlService = urlService;
        }

        public async Task<IEnumerable<SystemCategoryDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return categories.Select(MapToDto);
        }

        public async Task<SystemCategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("System category not found.");
            }
            return MapToDto(category);
        }

        public async Task<SystemCategoryDto> CreateAsync(CreateSystemCategoryDto dto)
        {
            if (await _repository.ExistsByNameAsync(dto.Name))
            {
                throw new ArgumentException("A system category with this name already exists.");
            }

            var category = new SystemCategory
            {
                Name = dto.Name,
                Description = dto.Description
            };

            if (dto.Image != null)
            {
                category.Image = await _fileStorageService.SaveImage(dto.Image, "system-categories");
            }

            await _repository.CreateAsync(category);
            return MapToDto(category);
        }

        public async Task UpdateAsync(int id, UpdateSystemCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("System category not found.");
            }

            if (await _repository.ExistsByNameAsync(dto.Name, id))
            {
                throw new ArgumentException("A system category with this name already exists.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;

            if (dto.Image != null)
            {
                await _fileStorageService.DeleteImage(category.Image);
                category.Image = await _fileStorageService.SaveImage(dto.Image, "system-categories");
            }

            await _repository.UpdateAsync(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("System category not found.");
            }

            category.IsActive = false;
            await _repository.UpdateAsync(category);
        }

        private SystemCategoryDto MapToDto(SystemCategory category)
        {
            return new SystemCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Image = string.IsNullOrEmpty(category.Image)? null: _urlService.GetAbsoluteUrl(category.Image)
            };
        }
    }
}
