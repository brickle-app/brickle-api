using BricklePlatform.Api.Application.ExceptionHandler;
using BricklePlatform.Api.Models;
using System.Net;

namespace BricklePlatform.Api.Middleware;

public class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IDictionary<Type, IExceptionHandler> exceptionHandlers)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger = logger;
    private readonly IDictionary<Type, IExceptionHandler> _exceptionHandlers = exceptionHandlers;

    public async Task Invoke(HttpContext context)
    {
        HeaderRequestModel headerRequestModel = new()
        {
            CorrelationId = context.Request.Headers["correlationId"].FirstOrDefault() ?? string.Empty,
            User = context.Request.Headers["user"].FirstOrDefault() ?? string.Empty,
            Source = context.Request.Headers["source"].FirstOrDefault() ?? string.Empty,
            RequestDate = DateTime.TryParse(context.Request.Headers["requestDate"].FirstOrDefault(), out var date) ? date : DateTime.UtcNow
        };

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandlerExceptionAsync(context, ex, headerRequestModel);
        }
    }

    private Task HandlerExceptionAsync(HttpContext context, Exception ex, HeaderRequestModel headerRequestModel)
    {
        string message = $"{ex.Message} - CorrelationId: {headerRequestModel.CorrelationId}, User: {headerRequestModel.User} - Source: {headerRequestModel.Source}, Date: {headerRequestModel.RequestDate}";
        _logger.LogError(ex, message);
        Task taskResult;
        Type exptionType = ex.GetType();

        if (_exceptionHandlers.TryGetValue(exptionType, out IExceptionHandler? exceptionHandler) && exceptionHandler != null)
        {
            taskResult = exceptionHandler.Handler(context, ex, headerRequestModel);
        }
        else
        {
            taskResult = new ExceptionHandlerBase()
                .SetResult(
                context,
                [ex.Message],
                HttpStatusCode.InternalServerError,
                headerRequestModel
            );
        }

        return taskResult;
    }
}