using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs.Categories;
using Habit_Tracker_Backend.Exceptions;
using Habit_Tracker_Backend.Models.Classes;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            return await _context.HabitCategories
                .Where(c => c.IsActive)
                .Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(long categoryId)
        {
            return await _context.HabitCategories
                .Where(c => c.CategoryId == categoryId && c.IsActive)
                .Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<long> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Check for duplicate category name
            var exists = await _context.HabitCategories
                .AnyAsync(c => c.CategoryName == dto.CategoryName.Trim() && c.IsActive);

            if (exists)
                throw new BadRequestException($"Category '{dto.CategoryName}' already exists");

            var category = new HabitCategory
            {
                CategoryName = dto.CategoryName.Trim(),
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.HabitCategories.Add(category);
            await _context.SaveChangesAsync();

            return category.CategoryId;
        }

        public async Task UpdateCategoryAsync(long categoryId, CreateCategoryDto dto)
        {
            var category = await _context.HabitCategories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.IsActive);

            if (category == null)
                throw new NotFoundException($"Category with ID {categoryId} not found");

            // Check for duplicate name (excluding current category)
            var duplicateExists = await _context.HabitCategories
                .AnyAsync(c => c.CategoryName == dto.CategoryName.Trim() 
                    && c.CategoryId != categoryId 
                    && c.IsActive);

            if (duplicateExists)
                throw new BadRequestException($"Category '{dto.CategoryName}' already exists");

            category.CategoryName = dto.CategoryName.Trim();
            category.Description = dto.Description;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(long categoryId)
        {
            var category = await _context.HabitCategories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
                throw new NotFoundException($"Category with ID {categoryId} not found");

            // Check if category has any habits
            var hasHabits = await _context.Habits
                .AnyAsync(h => h.CategoryId == categoryId);

            if (hasHabits)
                throw new BadRequestException("Cannot delete category that has associated habits");

            // Soft delete
            category.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
