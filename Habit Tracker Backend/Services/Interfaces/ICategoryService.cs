using Habit_Tracker_Backend.DTOs.Categories;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryResponseDto?> GetCategoryByIdAsync(long categoryId);
        Task<long> CreateCategoryAsync(CreateCategoryDto dto);
        Task UpdateCategoryAsync(long categoryId, CreateCategoryDto dto);
        Task DeleteCategoryAsync(long categoryId);
    }
}
