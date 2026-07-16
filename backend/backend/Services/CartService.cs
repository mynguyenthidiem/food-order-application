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
            if (!await _repository.FoodExistsAsync(dto.FoodId))
            {
                throw new Exception("Food not found.");
            }
            var cart = await _repository.GetByUserAndFoodAsync(userId, dto.FoodId);
            if (cart != null)
            {
                cart.Quantity += dto.Quantity;
                await _repository.UpdateAsync(cart);
                return MapToDto(cart);
            }
            cart = new Cart
            {
                UserId = userId,
                FoodId = dto.FoodId,
                Quantity = dto.Quantity,
                AddedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(cart);
            cart = await _repository.GetByIdAsync(cart.Id);
            return MapToDto(cart!);
        }

        public async Task<bool> UpdateCartAsync(int userId, int cartId, UpdateCartDto dto)
        {
            var cart = await _repository.GetByIdAsync(cartId);
            if (cart == null)
            {
                throw new Exception("Cart item not found.");
            }
            if (cart.UserId != userId)
            {
                throw new Exception("You are not allowed to update this cart item.");
            }
            cart.Quantity = dto.Quantity;
            await _repository.UpdateAsync(cart);
            return true;
        }

        public async Task<bool> DeleteCartAsync(int userId, int cartId)
        {
            var cart = await _repository.GetByIdAsync(cartId);
            if (cart == null){
                throw new Exception("Cart item not found.");
            }
            if (cart.UserId != userId)
            {
                throw new Exception("You are not allowed to delete this cart item.");
            }
            await _repository.DeleteAsync(cart);
            return true;
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