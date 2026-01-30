using Habit_Tracker_Backend.DTOs;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Habit_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup(RegisterDto dto)
        {
            return Ok(await _authService.RegisterAsync(dto));
        }

        [EnableRateLimiting("login-limiter")]
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            return Ok(await _authService.LoginAsync(dto));
        }

        [EnableRateLimiting("otp-limiter")]
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            return Ok(await _authService.ForgotPasswordAsync(dto));
        }

        [EnableRateLimiting("otp-limiter")]
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            return Ok(await _authService.ResetPasswordAsync(dto));
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new
            {
                message = "Logged out successfully"
            });
        }

        [EnableRateLimiting("otp-limiter")]
        [HttpPost("send-verification-email")]
        [AllowAnonymous]
        public async Task<IActionResult> SendVerificationEmail([FromBody] ForgotPasswordDto dto)
        {
            return Ok(await _authService.SendVerificationEmailAsync(dto.Email));
        }

        [EnableRateLimiting("otp-limiter")]
        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        {
            return Ok(await _authService.VerifyEmailAsync(dto));
        }
    }
}
