using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Food)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cart>> GetSelectedCartAsync(int userId, List<int> cartIds)
        {
            return await _context.Carts
                .Include(c => c.Food)
                .Where(c => c.UserId == userId &&
                            cartIds.Contains(c.Id))
                .ToListAsync();
        }

        public async Task ClearSelectedCartAsync(int userId, List<int> cartIds)
        {
            var carts = await _context.Carts
                .Where(c => c.UserId == userId &&
                            cartIds.Contains(c.Id))
                .ToListAsync();

            _context.Carts.RemoveRange(carts);
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Food)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public Task CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);

            return Task.CompletedTask;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(Order order)
        {
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
        }

        public Task AddOrderDetailAsync(OrderDetail orderDetail)
        {
            _context.OrderDetails.Add(orderDetail);

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Cart>> GetUserCartAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Food)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task ClearCartAsync(int userId)
        {
            var carts = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.Carts.RemoveRange(carts);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}