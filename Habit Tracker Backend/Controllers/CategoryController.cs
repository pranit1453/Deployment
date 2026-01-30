using Habit_Tracker_Backend.DTOs.Categories;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Habit_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize(Roles = "USER")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{categoryId:long}")]
        public async Task<IActionResult> GetCategoryById(long categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var categoryId = await _categoryService.CreateCategoryAsync(dto);

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { categoryId },
                new
                {
                    categoryId,
                    message = "Category created successfully"
                });
        }

        [HttpPut("{categoryId:long}")]
        public async Task<IActionResult> UpdateCategory(long categoryId, [FromBody] CreateCategoryDto dto)
        {
            await _categoryService.UpdateCategoryAsync(categoryId, dto);
            return Ok(new
            {
                success = true,
                message = "Category updated successfully"
            });
        }

        [HttpDelete("{categoryId:long}")]
        public async Task<IActionResult> DeleteCategory(long categoryId)
        {
            await _categoryService.DeleteCategoryAsync(categoryId);
            return Ok(new
            {
                success = true,
                message = "Category deleted successfully"
            });
        }
    }
}
