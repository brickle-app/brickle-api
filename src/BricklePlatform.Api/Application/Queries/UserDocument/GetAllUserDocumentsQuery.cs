using BricklePlatform.Domain.DTOs;
using MediatR;
using System.Collections.Generic;

namespace BricklePlatform.Api.Application.Queries.UserDocument;

public class GetAllUserDocumentsQuery : IRequest<IEnumerable<UserDocumentDto>>
{
    public string? Status { get; set; }

    public GetAllUserDocumentsQuery(string? status = null)
    {
        Status = status;
    }
}
