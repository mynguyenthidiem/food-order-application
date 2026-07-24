using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly AppDbContext _context;

        public RestaurantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Restaurant> Items, int TotalCount)> GetAll(int pageNumber, int pageSize)
        {
            var query = _context.Restaurants
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.Rating)
                .AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return (items, totalCount);
        }

        public async Task<Restaurant?> GetById(int id)
        {
            return await _context.Restaurants
                .Include(r => r.Categories.Where(c => c.IsActive)) 
                .Include(r => r.Foods.Where(f => f.Status == FoodStatus.Available)) 
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
        }

        public async Task Create(Restaurant restaurant)
        {
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Restaurant restaurant)
        {
            _context.Restaurants.Update(restaurant);
            await _context.SaveChangesAsync();
        }
        public async Task Delete(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
            {
                throw new Exception("Restaurant not found.");
            }

            restaurant.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRestaurantRatingAsync(int restaurantId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.Food.RestaurantId == restaurantId)
                .ToListAsync();

            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null) return;

            if (reviews.Any())
            {
                restaurant.TotalReviews = reviews.Count;
                restaurant.Rating = (double)Math.Round(reviews.Average(r => r.Rating), 1);
            }
            else
            {
                restaurant.TotalReviews = 0;
                restaurant.Rating = 0;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAllByOwnerAsync(int ownerId)
        {
            var restaurants = await _context.Restaurants
                .Where(r => r.OwnerId == ownerId && r.IsActive)
                .ToListAsync();

            foreach (var restaurant in restaurants)
            {
                restaurant.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }

    }
}