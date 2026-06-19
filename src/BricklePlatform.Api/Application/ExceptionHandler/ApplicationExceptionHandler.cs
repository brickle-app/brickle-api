using BricklePlatform.Api.Models;
using System.Net;

namespace BricklePlatform.Api.Application.ExceptionHandler;

public class ApplicationExceptionHandler : ExceptionHandlerBase, IExceptionHandler
{
    public Task Handler(HttpContext context, Exception exception, HeaderRequestModel headerRequestModel)
    {
        ApplicationException? ex = exception as ApplicationException;
        return SetResult(
            context,
            [ex?.Message ?? "Error en la capa de aplicación"],
            HttpStatusCode.BadRequest,
            headerRequestModel
        );
    }
}