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

        public async Task<OrderDto> GetOrderByIdAsync(int userId, int orderId)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                throw new Exception("Order not found.");

            if (order.UserId != userId)
                throw new Exception("You are not allowed to view this order.");

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
                var carts = (await _repository.GetSelectedCartAsync(userId, dto.CartIds)).ToList();
                if (!carts.Any())
                    throw new Exception("Cart is empty.");

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
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

                await _repository.ClearSelectedCartAsync(userId, dto.CartIds);

                await _repository.SaveChangesAsync();

                await transaction.CommitAsync();

                var createdOrder = await _repository.GetByIdAsync(order.Id);

                if (createdOrder == null)
                    throw new Exception("Failed to load created order.");

                return MapToDto(createdOrder);
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
                throw new Exception("Order not found.");

            if (order.UserId != userId)
                throw new Exception("You are not allowed to update this order.");

            if (order.Status != OrderStatus.Pending)
                throw new Exception("Only pending orders can be updated.");

            order.ShippingAddress = dto.ShippingAddress;
            order.PaymentMethod = dto.PaymentMethod;

            await _repository.UpdateOrderAsync(order);

            return true;
        }

        public async Task<bool> DeleteOrderAsync(int userId, int orderId)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                throw new Exception("Order not found.");

            if (order.UserId != userId)
                throw new Exception("You are not allowed to delete this order.");

            await _repository.DeleteOrderAsync(order);

            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
            {
                throw new Exception("Order not found.");
            }
            if (!IsValidStatus(dto.Status))
            {
                throw new Exception("Invalid order status.");
            }
            if (!CanChangeStatus(order.Status, dto.Status))
            {
                throw new Exception("Cannot change order status.");
            }
            order.Status = dto.Status;
            await _repository.UpdateOrderAsync(order);
            return true;
        }

        private bool IsValidStatus(OrderStatus status)
        {
            return Enum.IsDefined(typeof(OrderStatus), status);
        }

        private bool CanChangeStatus(OrderStatus currentStatus, OrderStatus newStatus)
        {
            switch (currentStatus)
            {
                case OrderStatus.Pending:
                    return newStatus == OrderStatus.Confirmed ||
                           newStatus == OrderStatus.Cancelled;

                case OrderStatus.Confirmed:
                    return newStatus == OrderStatus.Preparing;

                case OrderStatus.Preparing:
                    return newStatus == OrderStatus.Delivering;

                case OrderStatus.Delivering:
                    return newStatus == OrderStatus.Completed;

                case OrderStatus.Completed:
                case OrderStatus.Cancelled:
                    return false;

                default:
                    return false;
            }
        }
    }
}