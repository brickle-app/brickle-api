using BricklePlatform.Api.Models;
using System.Net;

namespace BricklePlatform.Api.Application.ExceptionHandler;

public class ArgumentExceptionHandler : ExceptionHandlerBase, IExceptionHandler
{
    public Task Handler(HttpContext context, Exception exception, HeaderRequestModel headerRequestModel)
    {
        ArgumentException? ex = exception as ArgumentException;
        return SetResult(context, [ex?.Message ?? "Argument error"], HttpStatusCode.BadRequest, headerRequestModel);
    }
}