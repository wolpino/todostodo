using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace todostodo.api.Middleware;

/// <summary>
/// Handles all unhandled exceptions in one place, logs them properly, 
/// and ensures consistent error responses without cluttering individual methods./// This ensures all unhandled exceptions are logged and a proper response is returned 
/// </summary>
/// <param name="logger">The logger to use for logging the exception.</param>
/// <returns>True if the exception was handled, false otherwise.</returns>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
