using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.UserDocumentSignature;

public record GetUserDocumentSignaturesQuery : IRequest<IEnumerable<UserDocumentSignatureDto>>
{
    public HeaderRequestModel Header { get; init; }
    public Guid UserId { get; init; }

    public GetUserDocumentSignaturesQuery(HeaderRequestModel header, Guid userId)
    {
        Header = header;
        UserId = userId;
    }
}
