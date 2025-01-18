using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Banco_VivesBank.User.Exceptions;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order { get; } = int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is UserNotFoundException userNotFoundException)
        {
            context.Result = new NotFoundObjectResult(userNotFoundException.Message);
            context.ExceptionHandled = true;
        }
        else if (context.Exception != null)
        {
            context.Result = new ObjectResult(new { message = "An internal error occurred." })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }
}