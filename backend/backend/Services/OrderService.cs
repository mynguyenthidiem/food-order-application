using backend.DTOs.Order;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IPaymentService _paymentService;

        public OrderService(IOrderRepository repository, IPaymentService paymentService)
        {
            _repository = repository;
            _paymentService = paymentService;
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
                throw new KeyNotFoundException("Order not found.");

            if (order.UserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to view this order.");

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

                Payment = order.Payment == null ? null : new PaymentResponseDto
                {
                    Id = order.Payment.Id,
                    OrderId = order.Payment.OrderId,
                    Amount = order.Payment.Amount,
                    Status = order.Payment.Status,
                    TransactionId = order.Payment.TransactionId,
                    CreatedAt = order.Payment.CreatedAt
                },

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
                    throw new InvalidOperationException("Cart is empty.");

                var firstCart = carts.First();
                int restaurantId = firstCart.Food?.RestaurantId
                                   ?? firstCart.Food?.Category?.RestaurantId
                                   ?? throw new InvalidOperationException("Invalid restaurant data in cart.");

                var order = new Order
                {
                    UserId = userId,
                    RestaurantId = restaurantId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ShippingAddress = dto.ShippingAddress,
                    PaymentMethod = dto.PaymentMethod,
                    TotalAmount = 0
                };

                await _repository.CreateOrderAsync(order);
                await _repository.SaveChangesAsync();

                decimal totalAmount = 0;

                foreach (var cart in carts)
                {
                    if (cart.Food == null || cart.Food.Status != FoodStatus.Available)
                    {
                        throw new InvalidOperationException(
                            $"Món ăn '{cart.Food?.Name ?? "đã chọn"}' hiện đã ngưng phục vụ. Vui lòng bỏ món này khỏi giỏ hàng.");
                    }

                    decimal price = cart.Food.Price;
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

                await _paymentService.CreatePayment(order.Id, order.TotalAmount, dto.PaymentMethod);

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

        public async Task UpdateOrderAsync(int userId, int orderId, UpdateOrderDto dto)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            if (order.UserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to update this order.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Only pending orders can be updated.");

            order.ShippingAddress = dto.ShippingAddress;
            if (order.Payment != null
               && order.Payment.Status == PaymentStatus.Pending)
            {
                order.Payment.Method = dto.PaymentMethod;
            }

            await _repository.UpdateOrderAsync(order);
        }

        public async Task DeleteOrderAsync(int userId, int orderId)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                throw new Exception("Order not found.");

            if (order.UserId != userId)
                throw new Exception("You are not allowed to delete this order.");

            if (order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Only pending orders can be cancelled.");
            }

            await _repository.DeleteOrderAsync(order);
        }

        public async Task UpdateOrderStatusAsync(int orderId, int currentUserId, bool isAdmin, UpdateOrderStatusDto dto)
        {
            var order = await _repository.GetByIdWithRestaurantAsync(orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            var restaurant = order.Restaurant;
            if (restaurant == null)
            {
                throw new InvalidOperationException("Restaurant info associated with this order could not be loaded.");
            }

            if (!isAdmin && restaurant.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to manage this order.");
            }

            if (!IsValidStatus(dto.Status))
            {
                throw new ArgumentException("Invalid order status.");
            }

            if (!CanChangeStatus(order.Status, dto.Status))
            {
                throw new InvalidOperationException("Cannot change order status.");
            }

            order.Status = dto.Status;

            await _repository.UpdateOrderAsync(order);
        }

        public async Task<IEnumerable<OrderDto>> GetRestaurantOrdersAsync(int ownerId)
        {
            var orders = await _repository.GetRestaurantOrdersAsync(ownerId);

            return orders.Select(MapToDto);
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _repository.GetAllOrdersAsync();

            return orders.Select(MapToDto);
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