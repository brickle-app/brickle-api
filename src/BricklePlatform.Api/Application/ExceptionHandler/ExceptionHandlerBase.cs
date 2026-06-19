using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Models;
using Newtonsoft.Json;
using System.Net;

namespace BricklePlatform.Api.Application.ExceptionHandler;

public class ExceptionHandlerBase
{
    private const string _RESPONSE_TITLE = "Error";
    private const string _RESPONSE_CONTENT_TYPE = "application/json";
    private const string _RESPONSE_TYPE = "Exception";

    public async Task SetResult(HttpContext context, string[] errorMessage, HttpStatusCode httpStatusCode, HeaderRequestModel headerRequestModel)
    {
        int status = (int)httpStatusCode;
        BadRequestResponseDto badRequestResponseDTO = new()
        {
            Title = _RESPONSE_TITLE,
            Status = status,
            Type = _RESPONSE_TYPE,
            TraceId = headerRequestModel.CorrelationId,
            Errors = new Dictionary<string, string[]>
        {
            { _RESPONSE_TITLE, errorMessage }
        },
        };
        BadRequestResponseDto errorDto = badRequestResponseDTO;
        context.Response.ContentType = _RESPONSE_CONTENT_TYPE;
        string response = JsonConvert.SerializeObject(errorDto);
        context.Response.StatusCode = status;
        context.Response.ContentType = _RESPONSE_CONTENT_TYPE;
        await context.Response.WriteAsync(response);
    }
}