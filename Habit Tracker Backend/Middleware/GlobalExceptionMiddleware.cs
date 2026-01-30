using Habit_Tracker_Backend.Exceptions;
using System.Text.Json;

namespace Habit_Tracker_Backend.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    error = ex.Message
                }));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    error = ex.Message
                }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    error = string.IsNullOrEmpty(ex.Message) ? "Resource not found." : ex.Message
                }));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    error = string.IsNullOrEmpty(ex.Message) ? "Invalid request." : ex.Message
                }));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    error = string.IsNullOrEmpty(ex.Message) ? "Invalid request." : ex.Message
                }));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    error = string.IsNullOrEmpty(ex.Message) ? "Invalid operation." : ex.Message
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    error = _env.IsDevelopment()
                        ? ex.Message
                        : "Something went wrong. Please try again later."
                }));
            }
        }
    }
}
