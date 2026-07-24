using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Restaurant)
                .Include(c => c.SystemCategory)
                .Where(c => c.IsActive)
                .OrderBy(x => x.SystemCategory.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Restaurant)
                .Include(c => c.SystemCategory)
                .Include(c => c.Foods.Where(f => f.Status == FoodStatus.Available))
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.Restaurant.IsActive);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            category.IsActive = false;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.Include(c => c.Restaurant).AnyAsync(x => x.Id == id && x.IsActive && x.Restaurant.IsActive);
        }

        public async Task<Category?> GetWithRestaurantAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Restaurant)
                .Include(c => c.SystemCategory)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive && c.Restaurant.IsActive);
        }

        public async Task<Restaurant?> GetRestaurantAsync(int restaurantId)
        {
            return await _context.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurantId && r.IsActive);
        }

        public async Task<SystemCategory?> GetSystemCategoryAsync(int systemCategoryId)
        {
            return await _context.SystemCategories
                .FirstOrDefaultAsync(sc => sc.Id == systemCategoryId && sc.IsActive);
        }

        public async Task<bool> ExistsByRestaurantAndSystemCategoryAsync(int restaurantId, int systemCategoryId, int? excludeCategoryId = null)
        {
            return await _context.Categories.AnyAsync(c =>
                c.RestaurantId == restaurantId &&
                c.SystemCategoryId == systemCategoryId &&
                c.IsActive &&
                (excludeCategoryId == null || c.Id != excludeCategoryId));
        }

        public async Task DeactivateFoodsByCategoryAsync(int categoryId)
        {
            var foods = await _context.Foods
                .Where(f => f.CategoryId == categoryId && f.Status == FoodStatus.Available)
                .ToListAsync();

            foreach (var food in foods)
            {
                food.Status = FoodStatus.Unavailable;
            }

            await _context.SaveChangesAsync();
        }
    }
}
