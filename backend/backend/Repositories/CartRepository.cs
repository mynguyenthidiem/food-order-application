using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        // Lấy toàn bộ giỏ hàng của User
        public async Task<IEnumerable<Cart>> GetUserCartAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Food)
                    .ThenInclude(f=>f.Restaurant)
                .Include(c=>c.Food)
                    .ThenInclude(f=>f.Category)
                .Where(c => c.UserId == userId 
                    && c.Food.Status==FoodStatus.Available
                    && c.Food.Restaurant.IsActive
                    && c.Food.Category.IsActive)
                .OrderByDescending(c => c.AddedAt)
                .ToListAsync();
        }

        // Tìm Cart theo Id
        public async Task<Cart?> GetByIdAsync(int id)
        {
            return await _context.Carts
                .Include(c => c.Food)
                    .ThenInclude(f=>f.Restaurant)
                .Include(c=>c.Food)
                    .ThenInclude(f=>f.Category)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Kiểm tra User đã có món này trong giỏ chưa
        public async Task<Cart?> GetByUserAndFoodAsync(int userId, int foodId)
        {
            return await _context.Carts
                .Include(c=>c.Food)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.FoodId == foodId);
        }

        // Kiểm tra Food tồn tại
        public async Task<bool> FoodExistsAsync(int foodId)
        {
            return await _context.Foods
                .Include(f=>f.Restaurant)
                .Include(f=>f.Category)
                .AnyAsync(f => f.Id == foodId
                            && f.Status == FoodStatus.Available
                            && f.Restaurant.IsActive
                            && f.Category.IsActive);
        }
        public async Task<Food?> GetFoodByIdAsync(int foodId)
        {
            return await _context.Foods
                .Include(f=>f.Restaurant)
                .Include(f=>f.Category)
                .FirstOrDefaultAsync(f => f.Id == foodId
                                        && f.Status == FoodStatus.Available
                                        && f.Restaurant.IsActive
                                        && f.Category.IsActive);
        }

        // Thêm vào Cart
        public async Task<Cart> AddAsync(Cart cart)
        {
            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();

            return cart;
        }

        // Cập nhật Cart
        public async Task UpdateAsync(Cart cart)
        {
            _context.Carts.Update(cart);

            await _context.SaveChangesAsync();
        }

        // Xóa 1 Cart
        public async Task DeleteAsync(Cart cart)
        {
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();
        }

        // Xóa toàn bộ Cart của User
        public async Task ClearAsync(int userId)
        {
            var carts = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.Carts.RemoveRange(carts);

            await _context.SaveChangesAsync();
        }
    }
}