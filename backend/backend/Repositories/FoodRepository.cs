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
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .Where(f => f.Status == FoodStatus.Available
                            && f.Category!.IsActive
                            && f.Restaurant!.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/foods/{id}
        public async Task<Food?> GetByIdAsync(int id)
        {
            return await _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == id
                                        && f.Status == FoodStatus.Available
                                        && f.Category!.IsActive
                                        && f.Restaurant!.IsActive);
        }

        // GET: api/foods/category/{id}
        public async Task<IEnumerable<Food>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .Where(f => f.CategoryId == categoryId
                        && f.Status == FoodStatus.Available
                        && f.Category!.IsActive
                        && f.Restaurant!.IsActive)
                .OrderBy(f => f.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/foods/search?keyword=
        public async Task<IEnumerable<Food>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<Food>();
            }

            var searchTerm = keyword.Trim();

            return await _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .Where(f => f.Status == FoodStatus.Available
                        && f.Category!.IsActive
                        && f.Restaurant!.IsActive
                        && (EF.Functions.Like(f.Name, $"%{searchTerm}%")
                            || (f.Description != null && EF.Functions.Like(f.Description, $"%{searchTerm}%"))))
                .OrderBy(f => f.Name)
                .AsNoTracking()
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
                .AnyAsync(f => f.Id == id
                            && f.Status == FoodStatus.Available
                            && f.Category!.IsActive
                            && f.Restaurant!.IsActive);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == categoryId
                            && c.IsActive
                            && c.Restaurant!.IsActive);
        }

        public async Task<Category?> GetCategoryWithRestaurantAsync(int categoryId)
        {
            return await _context.Categories
                .Include(c => c.Restaurant)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive && c.Restaurant!.IsActive);
        }
    }
}