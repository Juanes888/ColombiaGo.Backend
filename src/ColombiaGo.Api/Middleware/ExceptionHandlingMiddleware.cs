using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Middleware;

/// <summary>
/// Catches anything that escapes a controller action — genuinely unexpected failures, since
/// expected ones (not found, validation, conflict) are already handled as <c>Result</c> values
/// and never throw — and renders them as an RFC 7807 <see cref="ProblemDetails"/> body instead of
/// leaking a raw stack trace to the client.
/// </summary>
public sealed partial class ExceptionHandlingMiddleware
{
    private const int StatusCodeClientClosedRequest = 499;

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión (cambió de pantalla, recargó…): no es un fallo del servidor y nadie leerá la respuesta.
            context.Response.StatusCode = StatusCodeClientClosedRequest;
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex);
        }
        catch (Exception ex)
        {
            Log.UnhandledException(_logger, context.Request.Method, context.Request.Path, ex);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "InternalServerError", ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, int statusCode, string title, Exception ex)
    {
        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = _environment.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.",
            Instance = context.Request.Path,
        };

        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception processing {Method} {Path}")]
        public static partial void UnhandledException(ILogger logger, string method, string path, Exception ex);
    }
}
