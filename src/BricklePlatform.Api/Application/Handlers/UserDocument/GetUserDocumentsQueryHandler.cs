using BricklePlatform.Api.Application.Queries.UserDocument;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserDocument;

public class GetUserDocumentsQueryHandler : IRequestHandler<GetUserDocumentsQuery, IEnumerable<UserDocumentDto>>
{
    private readonly IUserDocumentRepository _userDocumentRepository;
    private readonly ILogger<GetUserDocumentsQueryHandler> _logger;

    public GetUserDocumentsQueryHandler(
        IUserDocumentRepository userDocumentRepository,
        ILogger<GetUserDocumentsQueryHandler> logger)
    {
        _userDocumentRepository = userDocumentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDocumentDto>> Handle(GetUserDocumentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving documents for user {UserId}. CorrelationId: {CorrelationId}",
            request.UserId, request.Header.CorrelationId);

        var documents = await _userDocumentRepository.GetByUserIdAsync(request.UserId);

        return documents.Select(d => new UserDocumentDto
        {
            Id = d.Id,
            UserId = d.UserId,
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
