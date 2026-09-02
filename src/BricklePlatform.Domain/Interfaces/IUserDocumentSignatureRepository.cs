using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface IUserDocumentSignatureRepository
{
    Task<UserDocumentSignature?> GetByUserAndDocumentTypeAsync(Guid userId, string documentType);
    Task<IEnumerable<UserDocumentSignature>> GetByUserIdAsync(Guid userId);
    Task<UserDocumentSignature> AddAsync(UserDocumentSignature signature);
    Task<UserDocumentSignature> UpdateAsync(UserDocumentSignature signature);
}
