using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs.User;
using Habit_Tracker_Backend.Exceptions;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileResponseDto> GetProfileAsync(long userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new NotFoundException("User not found.");

            return MapToProfileDto(user);
        }

        public async Task<UserProfileResponseDto> UpdateProfileAsync(long userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (await _context.Users.AnyAsync(u =>
                u.UserId != userId &&
                (u.Username == dto.Username || u.Email == dto.Email)))
            {
                throw new BadRequestException("Username or email is already in use.");
            }

            user.FirstName = dto.FirstName;
            user.MiddleName = dto.MiddleName;
            user.LastName = dto.LastName;
            user.Username = dto.Username;
            user.Email = dto.Email;
            user.MobileNumber = dto.MobileNumber;
            user.Dob = dto.Dob;

            await _context.SaveChangesAsync();

            return MapToProfileDto(user);
        }

        public async Task<bool> ToggleEmailNotificationsAsync(long userId, bool enabled)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            user.EmailNotificationsEnabled = enabled;
            await _context.SaveChangesAsync();
            return enabled;
        }

        private static UserProfileResponseDto MapToProfileDto(Models.Classes.User u)
        {
            var full = string.Join(" ", new[] { u.FirstName, u.MiddleName, u.LastName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            return new UserProfileResponseDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                MiddleName = u.MiddleName,
                LastName = u.LastName,
                FullName = full,
                Username = u.Username,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                Dob = u.Dob,
                EmailNotificationsEnabled = u.EmailNotificationsEnabled
            };
        }
    }
}
