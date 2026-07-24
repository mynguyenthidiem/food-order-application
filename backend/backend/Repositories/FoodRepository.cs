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

        public async Task<(List<Food> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .Where(f => f.Status == FoodStatus.Available
                            && f.Category!.IsActive
                            && f.Restaurant!.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return (items, totalCount);
        }

        public async Task<Food?> GetByIdAsync(int id)
        {
            return await _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.Status == FoodStatus.Available &&
                    f.Category != null &&
                    f.Category.IsActive &&
                    f.Restaurant != null &&
                    f.Restaurant.IsActive
                );
        }
        public async Task<Food?> GetFoodForManagementAsync(int id)
        {
            return await _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task<(List<Food> Items, int TotalCount)> GetByCategoryAsync(int categoryId, int pageNumber, int pageSize)
        {
            var query = _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .Where(f => f.CategoryId == categoryId
                        && f.Status == FoodStatus.Available
                        && f.Category!.IsActive
                        && f.Restaurant!.IsActive)
                .OrderBy(f => f.Name)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<Food> Items, int TotalCount)> GetBySystemCategoryAsync(int systemCategoryId, int pageNumber, int pageSize)
        {
            var query = _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .Where(f => f.Category!.SystemCategoryId == systemCategoryId
                        && f.Status == FoodStatus.Available
                        && f.Category!.IsActive
                        && f.Restaurant!.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }


        public async Task<(List<Food> Items, int TotalCount)> SearchAsync(string keyword, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return (new List<Food>(), 0);
            }

            var searchTerm = keyword.Trim();

            var query = _context.Foods
                .Include(f => f.Category)
                    .ThenInclude(c => c!.SystemCategory)
                .Include(f => f.Restaurant)
                .Where(f => f.Status == FoodStatus.Available
                            && f.Category != null
                            && f.Category.IsActive
                            && f.Restaurant != null
                            && f.Restaurant.IsActive
                            && ( f.Name.Contains(searchTerm) || (f.Description != null
                            && f.Description.Contains(searchTerm))))
                .OrderBy(f => f.Name)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // POST
        public async Task CreateAsync(Food food)
        {
            _context.Foods.Add(food);
            await _context.SaveChangesAsync();
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