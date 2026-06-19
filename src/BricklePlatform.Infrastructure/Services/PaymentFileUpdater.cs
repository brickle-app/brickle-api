using BricklePlatform.Domain.Common;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BricklePlatform.Infrastructure.Services;

public class PaymentFileUpdater : IEntityFileUpdater
{
    private readonly ILogger<PaymentFileUpdater> _logger;

    public PaymentFileUpdater(ILogger<PaymentFileUpdater> logger)
    {
        _logger = logger;
    }

    public Task UpdateEntityFileUrlAsync(Guid entityId, string fileUrl, string propertyName)
    {
        try
        {
            FileTypeMapping mapping = FileTypeMapping.Create("Payment", propertyName);

            if (mapping.EntityProperty.ToLower() != "receipt")
            {
                throw new DomainException($"Propiedad '{propertyName}' no válida para la entidad Payment. Solo se permite 'receipt'");
            }

            _logger.LogInformation(
                "Receipt uploaded successfully for user {UserId}. File URL: {FileUrl}",
                entityId, fileUrl);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating payment file URL for user {UserId}", entityId);
            throw;
        }
    }
}