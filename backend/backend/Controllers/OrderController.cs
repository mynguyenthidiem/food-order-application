using backend.DTOs.Order;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            // TODO: Lấy UserId từ JWT sau khi hoàn thành Authentication
            int userId = 1;

            var orders = await _service.GetOrdersAsync(userId);

            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            // TODO: Lấy UserId từ JWT sau khi hoàn thành Authentication
            int userId = 1;

            var order = await _service.GetOrderByIdAsync(userId, id);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            // TODO: Lấy UserId từ JWT sau khi hoàn thành Authentication
            int userId = 1;

            try
            {
                var order = await _service.CreateOrderAsync(userId, dto);

                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto dto)
        {
            // TODO: Lấy UserId từ JWT sau khi hoàn thành Authentication
            int userId = 1;

            var result = await _service.UpdateOrderAsync(userId, id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            return Ok(new
            {
                message = "Order updated successfully."
            });
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            // TODO: Lấy UserId từ JWT sau khi hoàn thành Authentication
            int userId = 1;

            var result = await _service.DeleteOrderAsync(userId, id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            return Ok(new
            {
                message = "Order deleted successfully."
            });
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
        {
            var result = await _service.UpdateOrderStatusAsync(id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            return Ok(new
            {
                message = "Order status updated successfully."
            });
        }
    }
}