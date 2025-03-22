using Microsoft.AspNetCore.Builder;

namespace WebAPI.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
