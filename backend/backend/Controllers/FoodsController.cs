using backend.DTOs.Food;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly IFoodService _service;

        public FoodsController(IFoodService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var foods = await _service.GetAllAsync();
            return Ok(foods);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var food = await _service.GetByIdAsync(id);
                return Ok(food);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("category/{id}")]
        public async Task<IActionResult> GetByCategory(int id)
        {
            var foods = await _service.GetByCategoryAsync(id);
            return Ok(foods);
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword is required." });
            }
            var foods = await _service.SearchAsync(keyword);
            return Ok(foods);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateFoodDto dto)
        {
            try
            {
                var food = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = food.Id }, food);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateFoodDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (!result)
                {
                    return NotFound(new { message = "Food not found." });
                }
                return Ok(new { message = "Food updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Food not found." });
                }
                return Ok(new { message = "Food deleted successfully." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Cannot delete food because it exists in cart or order." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
