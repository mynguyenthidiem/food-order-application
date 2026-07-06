using backend.DTOs.Order;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<OrderDto>> GetOrdersAsync(int userId)
        {
            var orders = await _repository.GetUserOrdersAsync(userId);

            return orders.Select(MapToDto);
        }
        public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                return null;

            if (order.UserId != userId)
                return null;

            return MapToDto(order);
        }
        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                ShippingAddress = order.ShippingAddress,

                OrderDetails = order.OrderDetails.Select(d => new OrderDetailDto
                {
                    FoodId = d.FoodId,
                    FoodName = d.Food?.Name ?? "",
                    Price = d.Price,
                    Quantity = d.Quantity,
                    SubTotal = d.SubTotal
                }).ToList()
            };
        }
        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            await using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                var carts = (await _repository.GetUserCartAsync(userId)).ToList();

                if (!carts.Any())
                    throw new Exception("Cart is empty.");

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    ShippingAddress = dto.ShippingAddress,
                    PaymentMethod = dto.PaymentMethod,
                    TotalAmount = 0
                };

                await _repository.CreateOrderAsync(order);

                // Lưu Order trước để lấy Order.Id
                await _repository.SaveChangesAsync();

                decimal totalAmount = 0;

                foreach (var cart in carts)
                {
                    decimal price = cart.Food!.Price;

                    decimal subTotal = price * cart.Quantity;

                    totalAmount += subTotal;

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        FoodId = cart.FoodId,
                        Quantity = cart.Quantity,
                        Price = price,
                        SubTotal = subTotal
                    };

                    await _repository.AddOrderDetailAsync(orderDetail);
                }

                order.TotalAmount = totalAmount;

                await _repository.ClearCartAsync(userId);

                await _repository.SaveChangesAsync();

                await transaction.CommitAsync();

                var createdOrder = await _repository.GetByIdAsync(order.Id);

                return MapToDto(createdOrder!);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<bool> UpdateOrderAsync(int userId, int orderId, UpdateOrderDto dto)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                return false;

            if (order.UserId != userId)
                return false;

            order.ShippingAddress = dto.ShippingAddress;
            order.PaymentMethod = dto.PaymentMethod;

            await _repository.UpdateOrderAsync(order);

            return true;
        }
        public async Task<bool> DeleteOrderAsync(int userId, int orderId)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                return false;

            if (order.UserId != userId)
                return false;

            await _repository.DeleteOrderAsync(order);

            return true;
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                return false;

            order.Status = dto.Status;

            await _repository.UpdateOrderAsync(order);

            return true;
        }
    }
}
