using BricklePlatform.Api.Application.Commands.UserDocument;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Constants;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserDocument;

public class UploadUserDocumentCommandHandler : IRequestHandler<UploadUserDocumentCommand, UserDocumentDto>
{
    private readonly IUserDocumentRepository _userDocumentRepository;
    private readonly IFileService _fileService;
    private readonly ILogger<UploadUserDocumentCommandHandler> _logger;

    public UploadUserDocumentCommandHandler(
        IUserDocumentRepository userDocumentRepository,
        IFileService fileService,
        ILogger<UploadUserDocumentCommandHandler> logger)
    {
        _userDocumentRepository = userDocumentRepository;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<UserDocumentDto> Handle(UploadUserDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading document {DocumentName} for user {UserId}. CorrelationId: {CorrelationId}",
            request.Body.Name, request.Body.UserId, request.Header.CorrelationId);

        if (!Domain.Entities.UserDocumentType.IsValid(request.Body.DocumentType))
        {
            throw new ApplicationException($"Tipo de documento inválido: {request.Body.DocumentType}");
        }

        var existingDocuments = await _userDocumentRepository.GetByUserIdAsync(request.Body.UserId);
        var existingDocument = existingDocuments.FirstOrDefault(d => d.DocumentType == request.Body.DocumentType);

        if (existingDocument != null)
        {
            if (existingDocument.Status == "APPROVED")
            {
                throw new ApplicationException("DUPLICATE_DOCUMENT: Este documento ya fue aprobado.");
            }

            if (existingDocument.Status == "PENDING")
            {
                throw new ApplicationException("DUPLICATE_DOCUMENT: Ya tienes este documento en revisión. Espera a que sea evaluado antes de volver a cargarlo.");
            }
        }

        using var stream = request.Body.File.OpenReadStream();
        var validation = await _fileService.ValidateFileAsync(stream, request.Body.File.FileName);

        if (!validation.IsValid)
        {
            throw new ApplicationException(validation.ErrorMessage ?? "Archivo inválido");
        }

        // Reset stream position after validation
        stream.Position = 0;

        // Upload to storage
        string documentUrl = await _fileService.UploadFileAsync(
            "USER_DOCUMENTS",
            request.Body.UserId,
            request.Body.Name,
            stream,
            request.Body.File.FileName);

        Domain.Entities.UserDocument document;
        if (existingDocument != null)
        {
            // Previous document was REJECTED: resubmit onto the same row instead of creating a duplicate.
            existingDocument.Resubmit(request.Body.Name, documentUrl);
            document = await _userDocumentRepository.UpdateAsync(existingDocument);
        }
        else
        {
            document = Domain.Entities.UserDocument.Create(
                request.Body.UserId,
                request.Body.Name,
                request.Body.DocumentType,
                documentUrl
            );

            await _userDocumentRepository.AddAsync(document);
        }

        _logger.LogInformation("Document {DocumentName} uploaded successfully for user {UserId}. CorrelationId: {CorrelationId}",
            request.Body.Name, request.Body.UserId, request.Header.CorrelationId);

        return new UserDocumentDto
        {
            Id = document.Id,
            UserId = document.UserId,
            Name = document.Name,
            DocumentType = document.DocumentType,
            DocumentUrl = document.DocumentUrl,
            Status = document.Status,
            Observation = document.Observation,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
