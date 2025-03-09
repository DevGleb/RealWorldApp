using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = exception switch
        {
            KeyNotFoundException => HttpStatusCode.NotFound, // 404
            UnauthorizedAccessException => HttpStatusCode.Unauthorized, // 401
            ArgumentException => HttpStatusCode.BadRequest, // 400
            _ => HttpStatusCode.InternalServerError // 500
        };

        response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            status = response.StatusCode,
            message = exception.Message
        };

        return response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
