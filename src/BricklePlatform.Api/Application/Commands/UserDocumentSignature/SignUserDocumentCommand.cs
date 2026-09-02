using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.UserDocumentSignature;

public record SignUserDocumentCommand : IRequest<UserDocumentSignatureDto>
{
    public HeaderRequestModel Header { get; init; }
    public SignUserDocumentRequestDto Body { get; init; }
    public string? IpAddress { get; init; }

    public SignUserDocumentCommand(HeaderRequestModel header, SignUserDocumentRequestDto body, string? ipAddress)
    {
        Header = header;
        Body = body;
        IpAddress = ipAddress;
    }
}
