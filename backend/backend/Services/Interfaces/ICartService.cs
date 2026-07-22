using backend.DTOs.Cart;

namespace backend.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartDto>> GetCartAsync(int userId);

        Task<CartDto> AddToCartAsync(int userId, AddCartDto dto);

        Task UpdateCartAsync(int userId, int cartId, UpdateCartDto dto);

        Task DeleteCartAsync(int userId, int cartId);

        Task ClearCartAsync(int userId);
    }
}