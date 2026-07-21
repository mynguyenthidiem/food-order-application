using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<List<Restaurant>> GetAll();

        Task<Restaurant?> GetById(int id);

        Task Create(Restaurant restaurant);

        Task Update(Restaurant restaurant);

        Task Delete(int id);
        Task UpdateRestaurantRatingAsync(int restaurantId);
    }
}