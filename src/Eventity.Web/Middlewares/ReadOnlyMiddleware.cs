using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Eventity.Web.Middlewares
{
    public class ReadOnlyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly bool _isReadOnlyMode;

        public ReadOnlyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _isReadOnlyMode = configuration.GetValue<bool>("READONLY_MODE", false);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Если режим только для чтения и запрос на изменение
            if (_isReadOnlyMode && IsWriteRequest(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    "{\"error\": \"Forbidden\", \"message\": \"This instance is read-only. Write operations are not allowed.\"}");
                return;
            }

            await _next(context);
        }

        private static bool IsWriteRequest(string method)
        {
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PATCH", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("DELETE", StringComparison.OrdinalIgnoreCase);
        }
    }
}