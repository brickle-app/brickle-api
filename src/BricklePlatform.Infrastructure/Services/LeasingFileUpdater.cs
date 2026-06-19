using BricklePlatform.Domain.Common;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.ValueObjects;

namespace BricklePlatform.Infrastructure.Services;

public class LeasingFileUpdater : IEntityFileUpdater
{
    private readonly ILeasingRepository _leasingRepository;

    public LeasingFileUpdater(ILeasingRepository leasingRepository)
    {
        _leasingRepository = leasingRepository;
    }

    public async Task UpdateEntityFileUrlAsync(Guid entityId, string fileUrl, string propertyName)
    {
        // Try to find leasing by ID first
        Leasing? leasing = await _leasingRepository.GetByIdAsync(entityId);

        if (leasing == null)
        {
            FileTypeMapping mapping = FileTypeMapping.Create("Leasing", propertyName);

            if (mapping.EntityProperty.ToLower() != "coverimageurl" &&
                mapping.EntityProperty.ToLower() != "miniatureimageurl" &&
                mapping.EntityProperty.ToLower() != "discoverimageurl")
            {
                throw new DomainException($"Propiedad '{propertyName}' no válida para la entidad Leasing");
            }
            Console.WriteLine($"File uploaded for future leasing creation: {fileUrl} - Property: {propertyName}");
            return;
        }

        FileTypeMapping entityMapping = FileTypeMapping.Create("Leasing", propertyName);

        switch (entityMapping.EntityProperty.ToLower())
        {
            case "coverimageurl":
                leasing.UpdateCoverImage(fileUrl);
                break;

            case "miniatureimageurl":
                leasing.UpdateMiniatureImage(fileUrl);
                break;

            case "discoverimageurl":
                leasing.UpdateDiscoverImage(fileUrl);
                break;

            default:
                throw new DomainException($"Propiedad '{propertyName}' no válida para la entidad Leasing");
        }

        await _leasingRepository.UpdateAsync(leasing);
    }
}