using FlowerShop.Infrastructure.Services;
using System.Text.Json;

namespace FlowerShop.API.Middleware;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (AppException ex)
        {
            logger.LogWarning("AppException: {Code} - {Message}", ex.Code, ex.Message);
            var statusCode = ex.Code switch
            {
                "NOT_FOUND" => 404,
                "UNAUTHORIZED" => 401,
                "FORBIDDEN" => 403,
                "CONFLICT" => 409,
                "RATE_LIMIT" => 429,
                _ => 400
            };
            ctx.Response.StatusCode = statusCode;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                error = new { code = ex.Code, message = ex.Message }
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                error = new { code = "INTERNAL_ERROR", message = "Внутренняя ошибка сервера" }
            }));
        }
    }
}
