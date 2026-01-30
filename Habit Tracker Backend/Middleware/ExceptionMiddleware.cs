using System.Net;
using System.Text.Json;

namespace Habit_Tracker_Backend.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var response = ex switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            context.Response.StatusCode = (int)response.Item1;

            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                message = response.Item2
            }));
        }
    }
}
