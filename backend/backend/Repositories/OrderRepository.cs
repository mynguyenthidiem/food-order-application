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

        public async Task<(List<Order> Items, int TotalCount)> GetUserOrdersAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Food)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return (items, totalCount);

        }

        public async Task<List<Cart>> GetSelectedCartAsync(int userId, List<int> cartIds)
        {
            return await _context.Carts
                .Include(c => c.Food)
                    .ThenInclude(f => f.Restaurant)
                .Include(c => c.Food)
                    .ThenInclude(f => f.Category)
                .Where(c => c.UserId == userId
                         && cartIds.Contains(c.Id)
                         && c.Food.Status == FoodStatus.Available
                         && c.Food.Restaurant.IsActive
                         && c.Food.Category.IsActive)
                .ToListAsync();
        }

        public async Task ClearSelectedCartAsync(int userId, List<int> cartIds)
        {
            var carts = await _context.Carts
                .Where(c => c.UserId == userId && cartIds.Contains(c.Id))
                .ToListAsync();

            _context.Carts.RemoveRange(carts);
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
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
            order.Status = OrderStatus.Cancelled;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public Task AddOrderDetailAsync(OrderDetail orderDetail)
        {
            _context.OrderDetails.Add(orderDetail);

            return Task.CompletedTask;
        }

        public async Task<List<Cart>> GetUserCartAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Food)
                .Where(c => c.UserId == userId && c.Food.Status == FoodStatus.Available)
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

        public async Task<Order?> GetByIdWithRestaurantAsync(int id)
        {
            return await _context.Orders
               .Include(o => o.Restaurant)
               .Include(o => o.Payment)
               .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Food)
               .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(List<Order> Items, int TotalCount)> GetRestaurantOrdersAsync(int ownerId, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                           .Include(o => o.OrderDetails)
                               .ThenInclude(od => od.Food)
                           .Include(o => o.Payment)
                           .Include(o => o.Restaurant)
                           .Where(o => o.Restaurant!.OwnerId == ownerId)
                           .OrderByDescending(o => o.OrderDate)
                           .AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return (items, totalCount);
        }

        public async Task<(List<Order> Items, int TotalCount)> GetAllOrdersAsync(int pageNumber, int pageSize)
        {
            var query = _context.Orders
                           .Include(o => o.Payment)
                           .Include(o => o.Restaurant)
                           .Include(o => o.OrderDetails)
                               .ThenInclude(d => d.Food)
                           .OrderByDescending(o => o.OrderDate)
                        .AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return (items, totalCount);
        }
    }
}