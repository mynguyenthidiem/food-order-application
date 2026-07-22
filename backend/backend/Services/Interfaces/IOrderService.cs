using backend.DTOs.Order;
using backend.Models;

namespace backend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersAsync(int userId);

        Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);

        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);

        Task UpdateOrderAsync(int userId, int orderId, UpdateOrderDto dto);

        Task DeleteOrderAsync(int userId, int orderId);

        Task UpdateOrderStatusAsync(int orderId, int currentUserId, bool isAdmin, UpdateOrderStatusDto dto);

        Task<IEnumerable<OrderDto>> GetRestaurantOrdersAsync(int ownerId);

        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    }
}