using Microsoft.AspNetCore.Diagnostics;
using MongoDB.Driver;
using System.Text.Json;
namespace AlertsAPI.Exceptions
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = StatusCodes.Status500InternalServerError;
            var message = "An internal server error occurred.";

            if (exception is MongoException mongoEx)
            {
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = $"Database error occurred: {mongoEx.Message}";
            }
            else if (exception is TimeoutException)
            {
                statusCode = StatusCodes.Status504GatewayTimeout;
                message = "The database operation timed out.";
            }
            else if (exception != null && _env.IsDevelopment())
            {
                message = exception.Message;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = statusCode,
                Message = message,
                Detailed = _env.IsDevelopment() ? exception?.StackTrace : null
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
