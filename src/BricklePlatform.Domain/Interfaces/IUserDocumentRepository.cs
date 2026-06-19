using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface IUserDocumentRepository
{
    Task<UserDocument?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserDocument>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserDocument>> GetAllAsync(string? status = null);
    Task<UserDocument> AddAsync(UserDocument document);
    Task<UserDocument> UpdateAsync(UserDocument document);
    Task<bool> DeleteAsync(Guid id);
}
