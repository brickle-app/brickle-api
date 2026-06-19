using System;
using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BricklePlatform.Api.Application.Commands.File;

public record UploadFileCommand : IRequest<string>
{
    public HeaderRequestModel Header { get; init; }
    public UploadFileRequestDto Body { get; init; }

    public UploadFileCommand(HeaderRequestModel header, UploadFileRequestDto body)
    {
        Header = header;
        Body = body;
    }
}