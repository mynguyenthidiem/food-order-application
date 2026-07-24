using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Review>> GetByFoodId(int foodId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.FoodId == foodId && r.User.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<Review> Items, int TotalCount)> GetByFoodIdPaged(int foodId, int pageNumber, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.FoodId == foodId && r.User.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Review?> GetById(int id)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.User.IsActive);
        }
        public async Task<Food?> GetFoodById(int foodId)
        {
            return await _context.Foods
                .Include(f => f.Category)
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == foodId
                                       && f.Status == FoodStatus.Available
                                       && f.Category.IsActive
                                       && f.Restaurant.IsActive);
        }
        public async Task<Review?> GetByUserAndFood(int userId, int foodId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.FoodId == foodId);
        }
        public async Task<bool> HasUserPurchasedFood(int userId, int foodId)
        {
            return await _context.OrderDetails
                .AnyAsync(od => od.FoodId == foodId &&
                                od.Order.UserId == userId &&
                                od.Order.Status == OrderStatus.Completed);
        }

        public async Task<Review> Create(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task Update(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Review review)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}