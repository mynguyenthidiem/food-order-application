using backend.DTOs.Page;
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

        public async Task<PagedResultDto<OrderDto>> GetOrdersAsync(int userId, PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.GetUserOrdersAsync(userId, pagination.PageNumber, pagination.PageSize);
            return new PagedResultDto<OrderDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
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

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            await using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                var carts = (await _repository.GetSelectedCartAsync(userId, dto.CartIds)).ToList();
                if (!carts.Any())
                {
                    throw new InvalidOperationException("Cart is empty.");
                }
                if (carts.Count != dto.CartIds.Count)
                {
                    throw new InvalidOperationException("Some selected cart items no longer exist.");
                }
                var firstCart = carts.First();
                if (carts.Any(c => c.Food == null))
                {
                    throw new InvalidOperationException("Some foods no longer exist.");
                }

                if (carts.Select(c => c.Food!.RestaurantId).Distinct().Count() > 1)
                {
                    throw new InvalidOperationException(
                        "You can only checkout foods from one restaurant at a time.");
                }
                var restaurant = carts.First().Food!.Restaurant;

                if (restaurant == null || !restaurant.IsActive)
                {
                    throw new InvalidOperationException("Restaurant is unavailable.");
                }

                int restaurantId = restaurant.Id;

                var now = DateTime.Now.TimeOfDay;

                if (DateTime.UtcNow < restaurant.OpenTime || DateTime.UtcNow > restaurant.CloseTime)
                {
                    throw new InvalidOperationException("Restaurant is currently closed.");
                }

                var order = new Order
                {
                    UserId = userId,
                    RestaurantId = restaurantId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ShippingAddress = dto.ShippingAddress,
                    PaymentMethod = dto.PaymentMethod,
                    DeliveryFee = restaurant.DeliveryFee,
                    TotalAmount = 0
                };

                await _repository.CreateOrderAsync(order);
                await _repository.SaveChangesAsync();

                decimal totalAmount = 0;

                foreach (var cart in carts)
                {
                    if (cart.Food == null || cart.Food.Status != FoodStatus.Available)
                    {
                        throw new InvalidOperationException($"Food '{cart.Food?.Name ?? "selected"}' is no longer available. Please remove this item from your cart.");
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

                order.TotalAmount = totalAmount + order.DeliveryFee;

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
            order.PaymentMethod = dto.PaymentMethod;
            if (order.Payment != null && order.Payment.Status == PaymentStatus.Pending)
            {
                order.Payment.Method = dto.PaymentMethod;
            }

            await _repository.UpdateOrderAsync(order);
        }

        public async Task DeleteOrderAsync(int userId, int orderId)
        {
            var order = await _repository.GetByIdAsync(orderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            if (order.UserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to delete this order.");

            if (order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Only pending orders can be cancelled.");
            }

            order.Status = OrderStatus.Cancelled;

            if (order.Payment != null)
            {
                order.Payment.Status = PaymentStatus.Cancelled;
            }

            await _repository.UpdateOrderAsync(order);
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

            if (dto.Status == OrderStatus.Completed
               && order.PaymentMethod == PaymentMethod.COD
               && order.Payment?.Status != PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Please confirm COD payment has been collected (CompletePayment) before marking this order as Completed.");
            }


            order.Status = dto.Status;

            await _repository.UpdateOrderAsync(order);
        }

        public async Task<PagedResultDto<OrderDto>> GetRestaurantOrdersAsync(int ownerId, PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.GetRestaurantOrdersAsync(ownerId, pagination.PageNumber, pagination.PageSize);

            return new PagedResultDto<OrderDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
        }

        public async Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.GetAllOrdersAsync(pagination.PageNumber, pagination.PageSize);

            return new PagedResultDto<OrderDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
        }



        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DeliveryFee = order.DeliveryFee,
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