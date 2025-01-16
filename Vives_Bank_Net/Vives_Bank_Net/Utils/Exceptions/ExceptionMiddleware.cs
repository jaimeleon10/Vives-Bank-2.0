using Vives_Bank_Net.Rest.Cliente.Exceptions;

namespace Vives_Bank_Net.Utils.Exceptions;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var statusCode = ex switch
        {
            ClienteNotFound => StatusCodes.Status404NotFound,
            ClienteBadRequest => StatusCodes.Status400BadRequest,
            ClienteConflict => StatusCodes.Status409Conflict
        };
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                Type = ex.GetType().Name
            };

            return context.Response.WriteAsJsonAsync(response);
    }
}