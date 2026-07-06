using backend.DTOs.Food;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly IFoodService _service;

        public FoodsController(IFoodService service)
        {
            _service = service;
        }

        // GET: api/foods
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var foods = await _service.GetAllAsync();
            return Ok(foods);
        }

        // GET: api/foods/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var food = await _service.GetByIdAsync(id);

            if (food == null)
                return NotFound(new
                {
                    message = "Food not found."
                });

            return Ok(food);
        }

        // GET: api/foods/category/2
        [HttpGet("category/{id}")]
        public async Task<IActionResult> GetByCategory(int id)
        {
            var foods = await _service.GetByCategoryAsync(id);

            return Ok(foods);
        }

        // GET: api/foods/search?keyword=burger
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new
                {
                    message = "Keyword is required."
                });
            }

            var foods = await _service.SearchAsync(keyword);

            return Ok(foods);
        }

        // POST: api/foods
        [HttpPost]
        public async Task<IActionResult> Create(CreateFoodDto dto)
        {
            try
            {
                var food = await _service.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = food.Id },
                    food);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/foods/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateFoodDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);

                if (!result)
                    return NotFound(new
                    {
                        message = "Food not found."
                    });

                return Ok(new
                {
                    message = "Food updated successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // DELETE: api/foods/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new
                {
                    message = "Food not found."
                });

            return Ok(new
            {
                message = "Food deleted successfully."
            });
        }
    }
}
