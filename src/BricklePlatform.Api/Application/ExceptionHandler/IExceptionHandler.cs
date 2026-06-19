using BricklePlatform.Api.Models;

namespace BricklePlatform.Api.Application.ExceptionHandler;

public interface IExceptionHandler
{
    Task Handler(HttpContext context, Exception exception, HeaderRequestModel headerRequestModel);
}