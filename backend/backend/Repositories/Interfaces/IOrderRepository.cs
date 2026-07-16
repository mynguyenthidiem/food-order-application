using backend.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        // Order
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<IEnumerable<Cart>> GetSelectedCartAsync(int userId, List<int> cartIds);
        Task ClearSelectedCartAsync(int userId, List<int> cartIds);
        Task<Order?> GetByIdAsync(int id);
        Task CreateOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(Order order);

        // OrderDetail
        Task AddOrderDetailAsync(OrderDetail orderDetail);

        // Cart
        Task<IEnumerable<Cart>> GetUserCartAsync(int userId);
        Task ClearCartAsync(int userId);

        // Transaction
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}