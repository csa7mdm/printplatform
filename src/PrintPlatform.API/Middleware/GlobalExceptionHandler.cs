using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.API.Middleware;

/// <summary>
/// Global exception handler that converts exceptions into standardized application/problem+json responses.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Exception occurred: {Message}. TraceId: {TraceId}",
            exception.Message,
            traceId);

        var (statusCode, title, detail) = exception switch
        {
            // Custom application exceptions
            NotFoundException nfe => (StatusCodes.Status404NotFound, "Not Found", nfe.Message),
            ConflictException ce => (StatusCodes.Status409Conflict, "Conflict", ce.Message),

            // Security / Auth
            UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "Unauthorized", "Access is denied."),
            ForbiddenAccessException _ => (StatusCodes.Status403Forbidden, "Forbidden", "You don't have permission to access this resource."),

            // Fallback for unhandled exceptions
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error has occurred. Please try again later.")
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Extensions = { ["traceId"] = traceId }
        }, cancellationToken: cancellationToken);

        return true;
    }
}

// Stubs for custom exception types referenced above
public class NotFoundException : Exception { public NotFoundException(string message) : base(message) { } }
public class ConflictException : Exception { public ConflictException(string message) : base(message) { } }
public class ForbiddenAccessException : Exception { public ForbiddenAccessException() : base() { } }
