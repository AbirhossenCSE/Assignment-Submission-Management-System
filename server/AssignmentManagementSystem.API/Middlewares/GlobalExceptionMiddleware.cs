using System.Net;
using System.Text.Json;
using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.Common.Exceptions;

namespace AssignmentManagementSystem.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception intercepted by GlobalExceptionMiddleware: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            BadRequestException => HttpStatusCode.BadRequest,
            ForbiddenException => HttpStatusCode.Forbidden,
            ConflictException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var message = statusCode == HttpStatusCode.InternalServerError
            ? "An unexpected error occurred. Please try again later or contact support."
            : exception.Message;

        var errors = statusCode == HttpStatusCode.InternalServerError
            ? new List<string> { "An unexpected internal server error occurred." }
            : new List<string> { exception.Message };

        var response = ApiResponse<object>.FailureResponse(
            message: message,
            errors: errors,
            statusCode: (int)statusCode
        );

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
