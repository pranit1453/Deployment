using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.Exceptions;
using Habit_Tracker_Backend.Models.Classes;
using Habit_Tracker_Backend.Models.Enums;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public OtpService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // --------------------------------------------------
        // SEND OTP
        // --------------------------------------------------
        public async Task SendOtpAsync(User user, OtpType type, OtpChannel channel)
        {
            // 1️⃣ Invalidate previous unused OTPs of same type
            var oldOtps = await _context.UserOtps
                .Where(o => o.UserId == user.UserId &&
                            o.OtpType == type &&
                            !o.IsUsed)
                .ToListAsync();

            foreach (var o in oldOtps)
                o.IsUsed = true;

            // 2️⃣ Generate secure OTP
            var otp = GenerateOtp();

            // 3️⃣ Store hashed OTP
            var entity = new UserOtp
            {
                UserId = user.UserId,
                OtpCodeHash = BCrypt.Net.BCrypt.HashPassword(otp),
                OtpType = type,
                Channel = channel,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0,
                IsUsed = false
            };

            _context.UserOtps.Add(entity);
            await _context.SaveChangesAsync();

            // 4️⃣ Send OTP (Email for now)
            //if (channel == OtpChannel.EMAIL)
            //{
            //    await _emailService.SendOtpAsync(user.Email, otp);
            //}
            if (channel == OtpChannel.EMAIL)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendOtpAsync(user.Email, otp);
                    }
                    catch (EmailSendFailedException)
                    {
                        
                    }
                });
            }
        }

        // --------------------------------------------------
        // VALIDATE OTP
        // --------------------------------------------------
        public async Task ValidateOtpAsync(long userId, string otp, OtpType type)
        {
            otp = otp.Trim();

            var entry = await _context.UserOtps
                .Where(o => o.UserId == userId &&
                            o.OtpType == type &&
                            !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (entry == null)
                throw new InvalidOtpException();

            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                entry.IsUsed = true;
                await _context.SaveChangesAsync();
                throw new OtpExpiredException();
            }

            if (entry.Attempts >= 5)
                throw new OtpAttemptsExceededException();

            if (!BCrypt.Net.BCrypt.Verify(otp, entry.OtpCodeHash))
            {
                entry.Attempts++;
                await _context.SaveChangesAsync();
                throw new InvalidOtpException();
            }

            // OTP valid → mark used
            entry.IsUsed = true;
            await _context.SaveChangesAsync();
        }

        // --------------------------------------------------
        // OTP GENERATOR (SECURE, 6-DIGIT)
        // --------------------------------------------------
        private static string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);

            var code = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return code.ToString("D6");
        }
    }
}
