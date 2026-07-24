using backend.DTOs.Page;
using backend.DTOs.Order;
using backend.Models;

namespace backend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<PagedResultDto<OrderDto>> GetOrdersAsync(int userId, PaginationParams pagination);

        Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);

        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);

        Task UpdateOrderAsync(int userId, int orderId, UpdateOrderDto dto);

        Task DeleteOrderAsync(int userId, int orderId);

        Task UpdateOrderStatusAsync(int orderId, int currentUserId, bool isAdmin, UpdateOrderStatusDto dto);

        Task<PagedResultDto<OrderDto>> GetRestaurantOrdersAsync(int ownerId, PaginationParams pagination);

        Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(PaginationParams pagination);
    }
}