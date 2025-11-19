using System.Net;
using System.Text.Json;
using backend.Constants;

namespace backend.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env; // Optional: show stacktrace only in Development

        public ErrorHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment env)
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the error
            _logger.LogError(exception, AppConstants.Logs.UnhandledException);

            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = AppConstants.Messages.InternalServerError;

            switch (exception)
            {
                case InvalidOperationException ex:
                    statusCode = HttpStatusCode.Conflict;       // 409
                    message = ex.Message;
                    break;

                case UnauthorizedAccessException ex:
                    statusCode = HttpStatusCode.Unauthorized;  // 401
                    message = ex.Message;
                    break;

                case KeyNotFoundException ex:
                    statusCode = HttpStatusCode.NotFound;      // 404
                    message = ex.Message;
                    break;

                // Add custom exceptions here if needed:
                // case BadRequestException ex: ...
            }

            context.Response.ContentType = AppConstants.MimeTypes.Json;
            context.Response.StatusCode = (int)statusCode;

            // Response object
            var response = new
            {
                success = false,
                status = statusCode,
                error = message,
                stackTrace = _env.IsDevelopment() ? exception.StackTrace : null
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
