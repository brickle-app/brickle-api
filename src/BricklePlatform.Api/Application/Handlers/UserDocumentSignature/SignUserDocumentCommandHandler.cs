using System.Text.Json;
using BricklePlatform.Api.Application.Commands.UserDocumentSignature;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserDocumentSignature;

public class SignUserDocumentCommandHandler : IRequestHandler<SignUserDocumentCommand, UserDocumentSignatureDto>
{
    private readonly IUserDocumentSignatureRepository _signatureRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SignUserDocumentCommandHandler> _logger;

    public SignUserDocumentCommandHandler(
        IUserDocumentSignatureRepository signatureRepository,
        IUserRepository userRepository,
        ILogger<SignUserDocumentCommandHandler> logger)
    {
        _signatureRepository = signatureRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDocumentSignatureDto> Handle(SignUserDocumentCommand request, CancellationToken cancellationToken)
    {
        var body = request.Body;

        if (!Domain.Entities.UserSignatureDocumentType.IsValid(body.DocumentType))
        {
            throw new ApplicationException($"Tipo de documento inválido: {body.DocumentType}");
        }

        if (body.SignaturePaths == null || body.SignaturePaths.Count == 0)
        {
            throw new ApplicationException("La firma no puede estar vacía.");
        }

        var user = await _userRepository.GetByIdAsync(body.UserId);
        if (user == null)
        {
            throw new ApplicationException($"Usuario no encontrado: {body.UserId}");
        }

        _logger.LogInformation(
            "Registrando firma del documento {DocumentType} para usuario {UserId}. CorrelationId: {CorrelationId}",
            body.DocumentType, body.UserId, request.Header.CorrelationId);

        var signatureData = JsonSerializer.Serialize(body.SignaturePaths);
        var existing = await _signatureRepository.GetByUserAndDocumentTypeAsync(body.UserId, body.DocumentType);

        Domain.Entities.UserDocumentSignature signature;
        if (existing != null)
        {
            existing.ReSign(body.DocumentVersion, signatureData, body.SignerName, request.IpAddress);
            signature = await _signatureRepository.UpdateAsync(existing);
        }
        else
        {
            signature = Domain.Entities.UserDocumentSignature.Create(
                body.UserId,
                body.DocumentType,
                body.DocumentVersion,
                signatureData,
                body.SignerName,
                request.IpAddress);

            await _signatureRepository.AddAsync(signature);
        }

        return new UserDocumentSignatureDto
        {
            Id = signature.Id,
            UserId = signature.UserId,
            DocumentType = signature.DocumentType,
            DocumentVersion = signature.DocumentVersion,
            SignerName = signature.SignerName,
            SignedAt = signature.SignedAt
        };
    }
}
