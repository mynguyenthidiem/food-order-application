using backend.DTOs.Cart;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repository;

        public CartService(ICartRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CartDto>> GetCartAsync(int userId)
        {
            var carts = await _repository.GetUserCartAsync(userId);
            return carts.Select(MapToDto);
        }
        public async Task<CartDto> AddToCartAsync(int userId, AddCartDto dto)
        {
            var food = await _repository.GetFoodByIdAsync(dto.FoodId);
            if (food == null)
            {
                throw new KeyNotFoundException("Food not found.");
            }

            if (food.Status != FoodStatus.Available)
            {
                throw new InvalidOperationException("This food item is currently unavailable.");
            }

            var existingCartItems = (await _repository.GetUserCartAsync(userId)).ToList();

            if (existingCartItems.Any())
            {
                var currentRestaurantId = existingCartItems.First().RestaurantId;

                if (currentRestaurantId != food.RestaurantId)
                {
                    throw new InvalidOperationException(
                        "Your cart already contains items from another restaurant. Please place or clear your current order before adding items from a new restaurant.");
                }
            }

            var cart = await _repository.GetByUserAndFoodAsync(userId, dto.FoodId);
            if (cart != null)
            {
                cart.Quantity += dto.Quantity;
                await _repository.UpdateAsync(cart);

                var updatedCart = await _repository.GetByIdAsync(cart.Id);
                return MapToDto(updatedCart ?? cart);
            }

            cart = new Cart
            {
                UserId = userId,
                FoodId = dto.FoodId,
                RestaurantId = food.RestaurantId,
                Quantity = dto.Quantity,
                AddedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(cart);

            cart = await _repository.GetByIdAsync(cart.Id);
            if (cart == null)
            {
                throw new Exception("Failed to load the created cart item.");
            }

            return MapToDto(cart);
        }

        public async Task UpdateCartAsync(int userId, int cartId, UpdateCartDto dto)
        {
            var cart = await _repository.GetByIdAsync(cartId);
            if (cart == null)
            {
                throw new KeyNotFoundException("Cart item not found.");
            }
            if (cart.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to update this cart item.");
            }
            cart.Quantity = dto.Quantity;
            await _repository.UpdateAsync(cart);
        }

        public async Task DeleteCartAsync(int userId, int cartId)
        {
            var cart = await _repository.GetByIdAsync(cartId);
            if (cart == null)
            {
                throw new KeyNotFoundException("Cart item not found.");
            }
            if (cart.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to delete this cart item.");
            }
            await _repository.DeleteAsync(cart);
        }

        public async Task ClearCartAsync(int userId)
        {
            await _repository.ClearAsync(userId);
        }

        private static CartDto MapToDto(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                FoodId = cart.FoodId,
                FoodName = cart.Food?.Name ?? "",
                Price = cart.Food?.Price ?? 0,
                Image = cart.Food?.Image,
                Quantity = cart.Quantity,
                TotalPrice = (cart.Food?.Price ?? 0) * cart.Quantity,
                AddedAt = cart.AddedAt
            };
        }
    }
}