using BricklePlatform.Api.Application.Queries.UserDocumentSignature;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserDocumentSignature;

public class GetUserDocumentSignaturesQueryHandler
    : IRequestHandler<GetUserDocumentSignaturesQuery, IEnumerable<UserDocumentSignatureDto>>
{
    private readonly IUserDocumentSignatureRepository _signatureRepository;
    private readonly ILogger<GetUserDocumentSignaturesQueryHandler> _logger;

    public GetUserDocumentSignaturesQueryHandler(
        IUserDocumentSignatureRepository signatureRepository,
        ILogger<GetUserDocumentSignaturesQueryHandler> logger)
    {
        _signatureRepository = signatureRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDocumentSignatureDto>> Handle(
        GetUserDocumentSignaturesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving document signatures for user {UserId}. CorrelationId: {CorrelationId}",
            request.UserId, request.Header.CorrelationId);

        var signatures = await _signatureRepository.GetByUserIdAsync(request.UserId);

        return signatures.Select(s => new UserDocumentSignatureDto
        {
            Id = s.Id,
            UserId = s.UserId,
            DocumentType = s.DocumentType,
            DocumentVersion = s.DocumentVersion,
            SignerName = s.SignerName,
            SignedAt = s.SignedAt
        });
    }
}
