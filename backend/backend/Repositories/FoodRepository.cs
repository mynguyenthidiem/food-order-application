using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly AppDbContext _context;

        public FoodRepository(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/foods
        public async Task<IEnumerable<Food>> GetAllAsync()
        {
            return await _context.Foods
                .Include(f => f.Category)
                .Where(f => f.Status != FoodStatus.Unavailable)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        // GET: api/foods/{id}
        public async Task<Food?> GetByIdAsync(int id)
        {
            return await _context.Foods
                .Include(f => f.Category)

                // Load dữ liệu đang tham chiếu Food
                .Include(f => f.OrderDetails)

                .Include(f => f.Carts)

                .FirstOrDefaultAsync(f => f.Id == id);
        }

        // GET: api/foods/category/{id}
        public async Task<IEnumerable<Food>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Foods
                .Include(f => f.Category)
                .Where(f => f.CategoryId == categoryId && f.Status != FoodStatus.Unavailable)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        // GET: api/foods/search?keyword=
        public async Task<IEnumerable<Food>> SearchAsync(string keyword)
        {
            keyword = keyword.Trim().ToLower();

            return await _context.Foods
                .Include(f => f.Category)
                .Where(f => f.Status != FoodStatus.Unavailable && (
                    f.Name.ToLower().Contains(keyword) ||
                    (f.Description != null &&
                     f.Description.ToLower().Contains(keyword))))
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        // POST
        public async Task<Food> CreateAsync(Food food)
        {
            _context.Foods.Add(food);

            await _context.SaveChangesAsync();

            return food;
        }

        // PUT
        public async Task UpdateAsync(Food food)
        {
            _context.Foods.Update(food);

            await _context.SaveChangesAsync();
        }

        // DELETE
        public async Task DeleteAsync(Food food)
        {
            food.Status = FoodStatus.Unavailable;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Foods
                .AnyAsync(f => f.Id == id);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c =>
                    c.Id == categoryId
                    &&
                    c.IsActive);
        }
    }
}
