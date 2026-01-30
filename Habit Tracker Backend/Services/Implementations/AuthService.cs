using Habit_Tracker_Backend.Configurations;
using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs;
using Habit_Tracker_Backend.Exceptions;
using Habit_Tracker_Backend.Models.Classes;
using Habit_Tracker_Backend.Models.Enums;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IOtpService _otpService;
        private readonly IJwtService _jwtService;
        private readonly JwtOptions _jwtOptions;

        public AuthService(
            AppDbContext context,
            IOtpService otpService,
            IJwtService jwtService,
            IOptions<JwtOptions> jwtOptions)
        {
            _context = context;
            _otpService = otpService;
            _jwtService = jwtService;
            _jwtOptions = jwtOptions.Value;
        }

        // ---------------- REGISTER ----------------
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u =>
                u.Username == dto.Username || u.Email == dto.Email))
            {
                throw new BadRequestException("Account already exists");
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Username = dto.Username,
                Email = dto.Email,
                MobileNumber = dto.MobileNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Dob = dto.Dob,
                Role = Role.USER,
                IsActive = true,
                IsMobileVerified = false,
                IsEmailVerified = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send verification email after registration
            await _otpService.SendOtpAsync(
                user,
                OtpType.EMAIL_VERIFICATION,
                OtpChannel.EMAIL
            );

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful. Please verify your email."
            };
        }

        // ---------------- LOGIN ----------------
        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u =>
                    u.Username == dto.Username ||
                    u.Email == dto.Username);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid credentials");

            if (!user.IsActive)
                throw new UnauthorizedException("Account is inactive");

            return new LoginResultDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role.ToString(),
                Token = _jwtService.GenerateToken(user),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes),
                User = new DTOs.LoginUserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role.ToString(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                }
            };
        }

        // ---------------- FORGOT PASSWORD ----------------
        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _context.Users
        .SingleOrDefaultAsync(u => u.Email == dto.Email);

            //  Prevent email enumeration
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "If the account exists, an OTP has been sent"
                };
            }

            // Invalidate old OTPs 
            var oldOtps = await _context.UserOtps
                .Where(o => o.UserId == user.UserId && !o.IsUsed)
                .ToListAsync();

            foreach (var otp in oldOtps)
            {
                otp.IsUsed = true;
            }

            await _context.SaveChangesAsync();

            //  Send new OTP
            await _otpService.SendOtpAsync(
                user,
                OtpType.FORGOT_PASSWORD,
                OtpChannel.EMAIL
            );

            return new AuthResponseDto
            {
                Success = true,
                Message = "If the account exists, an OTP has been sent"
            };
        }

        // ---------------- RESET PASSWORD ----------------
        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new BadRequestException("Invalid request");

            await _otpService.ValidateOtpAsync(
                user.UserId,
                dto.Otp,
                OtpType.FORGOT_PASSWORD);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Password reset successful"
            };
        }

        // ---------------- SEND VERIFICATION EMAIL ----------------
        public async Task<AuthResponseDto> SendVerificationEmailAsync(string email)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "If the account exists, a verification email has been sent"
                };
            }

            if (user.IsEmailVerified)
            {
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Email is already verified"
                };
            }

            // Invalidate old verification OTPs
            var oldOtps = await _context.UserOtps
                .Where(o => o.UserId == user.UserId &&
                           o.OtpType == OtpType.EMAIL_VERIFICATION &&
                           !o.IsUsed)
                .ToListAsync();

            foreach (var otp in oldOtps)
            {
                otp.IsUsed = true;
            }

            await _context.SaveChangesAsync();

            // Send verification OTP
            await _otpService.SendOtpAsync(
                user,
                OtpType.EMAIL_VERIFICATION,
                OtpChannel.EMAIL
            );

            return new AuthResponseDto
            {
                Success = true,
                Message = "Verification email sent"
            };
        }

        // ---------------- VERIFY EMAIL ----------------
        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new BadRequestException("Invalid request");

            if (user.IsEmailVerified)
            {
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Email is already verified"
                };
            }

            await _otpService.ValidateOtpAsync(
                user.UserId,
                dto.Otp,
                OtpType.EMAIL_VERIFICATION);

            user.IsEmailVerified = true;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Email verified successfully"
            };
        }
    }
}
