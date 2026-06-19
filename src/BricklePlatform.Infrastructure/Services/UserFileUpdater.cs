using BricklePlatform.Domain.Common;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.ValueObjects;

namespace BricklePlatform.Infrastructure.Services;

public class UserFileUpdater : IEntityFileUpdater
{
    private readonly IUserRepository _userRepository;

    public UserFileUpdater(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task UpdateEntityFileUrlAsync(Guid entityId, string fileUrl, string propertyName)
    {
        User? user = await _userRepository.GetByIdAsync(entityId);
        if (user == null)
            throw new DomainException($"Usuario con ID {entityId} no encontrado");

        FileTypeMapping mapping = FileTypeMapping.Create("User", propertyName);

        switch (mapping.EntityProperty.ToLower())
        {
            case "profileimageurl":
                user.UpdateProfilePicture(fileUrl);
                break;

            default:
                throw new DomainException($"Propiedad '{propertyName}' no válida para la entidad User");
        }

        await _userRepository.UpdateAsync(user);
    }
}