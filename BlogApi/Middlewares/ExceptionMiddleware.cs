using BlogApi.Helpers;
using System.Net;
using System.Text.Json;

namespace BlogApi.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
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
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Tạo phản hồi theo chuẩn ApiResponse của nhatdev
                var response = _env.IsDevelopment()
                    ? new ApiResponse<string>("Global Error", ex.Message, new List<string> { ex.StackTrace?.ToString() ?? "" })
                    : new ApiResponse<string>("Global Error", "Đã có lỗi hệ thống xảy ra, vui lòng thử lại sau.");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
