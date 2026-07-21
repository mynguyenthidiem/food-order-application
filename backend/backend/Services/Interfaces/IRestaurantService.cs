using backend.DTOs.Restaurant;

namespace backend.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<List<RestaurantDto>> GetAll();
        Task<RestaurantDto?> GetById(int id);
        Task<RestaurantDto> Create(int ownerId,CreateRestaurantDto dto);
        Task Update(int id,int ownerId, bool isAdmin, UpdateRestaurantDto dto);
        Task Delete(int id, int ownerId, bool isAdmin);
    }
}