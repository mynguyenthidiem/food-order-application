using backend.DTOs.Order;

namespace backend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersAsync(int userId);

        Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);

        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);

        Task<bool> UpdateOrderAsync(int userId, int orderId, UpdateOrderDto dto);

        Task<bool> DeleteOrderAsync(int userId, int orderId);

        Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);
    }
}