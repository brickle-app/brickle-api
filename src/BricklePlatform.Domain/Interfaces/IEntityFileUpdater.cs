namespace BricklePlatform.Domain.Interfaces;

public interface IEntityFileUpdater
{
    Task UpdateEntityFileUrlAsync(Guid entityId, string fileUrl, string fileType);
}