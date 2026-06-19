using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.UserDocument;

public record UploadUserDocumentCommand : IRequest<UserDocumentDto>
{
    public HeaderRequestModel Header { get; init; }
    public UploadUserDocumentRequestDto Body { get; init; }

    public UploadUserDocumentCommand(HeaderRequestModel header, UploadUserDocumentRequestDto body)
    {
        Header = header;
        Body = body;
    }
}
