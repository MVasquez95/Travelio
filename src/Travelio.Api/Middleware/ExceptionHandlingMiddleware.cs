using Microsoft.AspNetCore.Mvc;

namespace Travelio.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled request error for {Method} {Path}", context.Request.Method, context.Request.Path);
            var status = exception switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
            await Results.Problem(statusCode: status, title: exception.Message, type: $"https://httpstatuses.com/{status}").ExecuteAsync(context);
        }
    }
}
