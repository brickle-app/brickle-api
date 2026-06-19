using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.UserDocument;

public record GetUserDocumentsQuery : IRequest<IEnumerable<UserDocumentDto>>
{
    public HeaderRequestModel Header { get; init; }
    public Guid UserId { get; init; }

    public GetUserDocumentsQuery(HeaderRequestModel header, Guid userId)
    {
        Header = header;
        UserId = userId;
    }
}
