using backend.DTOs.Cart;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;

        public CartController(ICartService service)
        {
            _service = service;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            // TODO: Lấy UserId từ JWT sau khi hoàn thành Authentication
            int userId = 1;

            var carts = await _service.GetCartAsync(userId);

            return Ok(carts);
        }

        // POST: api/cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddCartDto dto)
        {
            // TODO: Lấy UserId từ JWT
            int userId = 1;

            try
            {
                var cart = await _service.AddToCartAsync(userId, dto);

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/cart/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(int id, UpdateCartDto dto)
        {
            // TODO: Lấy UserId từ JWT
            int userId = 1;

            var result = await _service.UpdateCartAsync(userId, id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Cart item not found."
                });
            }

            return Ok(new
            {
                message = "Cart updated successfully."
            });
        }

        // DELETE: api/cart/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            // TODO: Lấy UserId từ JWT
            int userId = 1;

            var result = await _service.DeleteCartAsync(userId, id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Cart item not found."
                });
            }

            return Ok(new
            {
                message = "Cart deleted successfully."
            });
        }

        // DELETE: api/cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            // TODO: Lấy UserId từ JWT
            int userId = 1;

            await _service.ClearCartAsync(userId);

            return Ok(new
            {
                message = "Cart cleared successfully."
            });
        }
    }
}