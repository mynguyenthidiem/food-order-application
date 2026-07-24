using backend.DTOs.Page;
using backend.DTOs.Restaurant;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUrlService _urlService;

        public RestaurantService(IRestaurantRepository repository, IFileStorageService fileStorageService, IUrlService urlService)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _urlService = urlService;
        }

        public async Task<PagedResultDto<RestaurantDto>> GetAll(PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.GetAll(pagination.PageNumber,pagination.PageSize);
            return new PagedResultDto<RestaurantDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber,pagination.PageSize);
        }

        public async Task<RestaurantDto?> GetById(int id)
        {
            var restaurant = await _repository.GetById(id);
            if (restaurant == null)
            {
                throw new KeyNotFoundException("Restaurant not found.");
            }
            return MapToDto(restaurant);
        }

        public async Task<RestaurantDto> Create(int ownerId, CreateRestaurantDto dto)
        {
            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Address = dto.Address,
                Description = dto.Description,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                OpenTime = dto.OpenTime,
                CloseTime = dto.CloseTime,
                DeliveryFee = dto.DeliveryFee,
                OwnerId = ownerId,
                IsActive = true,
                Rating = 0,
                TotalReviews = 0,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Image != null)
            {
                restaurant.ImageUrl = await _fileStorageService.SaveImage(dto.Image, "restaurants");
            }

            await _repository.Create(restaurant);

            return MapToDto(restaurant);
        }

        public async Task Update(int id, int ownerId, bool isAdmin, UpdateRestaurantDto dto)
        {
            var restaurant = await _repository.GetById(id);
            if (restaurant == null)
            {
                throw new KeyNotFoundException("Restaurant not found.");
            }

            if (!isAdmin && restaurant.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("You are not allowed.");
            }

            restaurant.Name = dto.Name;
            restaurant.Address = dto.Address;
            restaurant.Description = dto.Description;
            restaurant.PhoneNumber = dto.PhoneNumber;
            restaurant.Email = dto.Email;
            restaurant.OpenTime = dto.OpenTime;
            restaurant.CloseTime = dto.CloseTime;
            restaurant.DeliveryFee = dto.DeliveryFee;

            if (dto.Image != null)
            {
                // Tránh lỗi xoá file khi ImageUrl đang null hoặc rỗng
                if (!string.IsNullOrEmpty(restaurant.ImageUrl))
                {
                    await _fileStorageService.DeleteImage(restaurant.ImageUrl);
                }

                restaurant.ImageUrl = await _fileStorageService.SaveImage(dto.Image, "restaurants");
            }

            await _repository.Update(restaurant);
        }

        public async Task Delete(int id, int ownerId, bool isAdmin)
        {
            var restaurant = await _repository.GetById(id);
            if (restaurant == null)
            {
                throw new KeyNotFoundException("Restaurant not found.");
            }

            if (!isAdmin && restaurant.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("You are not allowed.");
            }

            restaurant.IsActive = false;
            await _repository.Update(restaurant);
        }

        public async Task SetActiveStatus (int id, bool isActive)
        {
            var restaurant = await _repository.GetById(id);
            if (restaurant == null)
            {
                throw new KeyNotFoundException("Restaurant not found.");
            }
            restaurant.IsActive = isActive;
            await _repository.Update(restaurant);
        }

        private  RestaurantDto MapToDto(Restaurant restaurant)
        {
            return new RestaurantDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
                Description = restaurant.Description,
                ImageUrl = string.IsNullOrEmpty(restaurant.ImageUrl) ? null : _urlService.GetAbsoluteUrl(restaurant.ImageUrl),
                PhoneNumber = restaurant.PhoneNumber,
                Email = restaurant.Email,
                OpenTime = restaurant.OpenTime,
                CloseTime = restaurant.CloseTime,
                DeliveryFee = restaurant.DeliveryFee,
                Rating = restaurant.Rating,
                TotalReviews = restaurant.TotalReviews,
                IsActive = restaurant.IsActive,
                CreatedAt = restaurant.CreatedAt,
                OwnerId = restaurant.OwnerId
            };
        }
    }
}