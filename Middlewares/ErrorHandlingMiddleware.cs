using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace RealWorldApp.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            object errorBody;
            int statusCode;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    errorBody = new
                    {
                        errors = validationException.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray()
                            )
                    };
                    break;

                case KeyNotFoundException _:
                    statusCode = (int)HttpStatusCode.NotFound;
                    errorBody = new
                    {
                        errors = new
                        {
                            body = new[] { "Resource not found" }
                        }
                    };
                    break;

                case UnauthorizedAccessException _:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    errorBody = new
                    {
                        errors = new
                        {
                            body = new[] { "Unauthorized access" }
                        }
                    };
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    errorBody = new
                    {
                        errors = new
                        {
                            body = new[] { "An unexpected error occurred." }
                        }
                    };
                    break;
            }

            context.Response.StatusCode = statusCode;
            var jsonResponse = JsonSerializer.Serialize(errorBody);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
