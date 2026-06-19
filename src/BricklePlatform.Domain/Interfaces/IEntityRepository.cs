namespace BricklePlatform.Domain.Interfaces;

public interface IEntityRepository
{
    Task UpdateEntityPropertyAsync(Guid entityId, string propertyName, string value);
}