using BricklePlatform.Api.Application.Queries.UserDocument;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BricklePlatform.Api.Application.Handlers.UserDocument;

public class GetAllUserDocumentsQueryHandler : IRequestHandler<GetAllUserDocumentsQuery, IEnumerable<UserDocumentDto>>
{
    private readonly IUserDocumentRepository _repository;

    public GetAllUserDocumentsQueryHandler(IUserDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserDocumentDto>> Handle(GetAllUserDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _repository.GetAllAsync(request.Status);

        return documents.Select(d => new UserDocumentDto
        {
            Id = d.Id,
            UserId = d.UserId,
            UserName = d.User != null ? $"{d.User.FirstName} {d.User.LastName}" : "Unknown",
            UserEmail = d.User?.Email ?? "Unknown",
            Name = d.Name,
            DocumentType = d.DocumentType,
            DocumentUrl = d.DocumentUrl,
            Status = d.Status,
            Observation = d.Observation,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        });
    }
}
