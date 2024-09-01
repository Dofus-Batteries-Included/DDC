using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DDC.Api.Exceptions;

class ExceptionHandler : IExceptionHandler
{
    readonly IProblemDetailsService _problemDetailsService;

    public ExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!Handle(exception, out int statusCode, out string detail))
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;
        await _problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Title = "An error occurred while processing your request.",
                    Status = statusCode,
                    Detail = detail
                },
                Exception = exception
            }
        );

        return true;
    }

    static bool Handle(Exception exception, out int statusCode, out string detail)
    {
        switch (exception)
        {
            case BadRequestException:
                statusCode = StatusCodes.Status400BadRequest;
                detail = exception.Message;
                return true;
            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                detail = exception.Message;
                return true;
            default:
                statusCode = 0;
                detail = "";
                return false;
        }
    }
}
