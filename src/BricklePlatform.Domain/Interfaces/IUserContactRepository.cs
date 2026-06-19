using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface IUserContactRepository
{
    Task<IEnumerable<UserContact>> GetContactsByUserIdAsync(Guid userId);
    Task<UserContact?> GetContactAsync(Guid userId, Guid contactId);
    Task<UserContact> AddContactAsync(UserContact userContact);
    Task DeleteContactAsync(Guid userId, Guid contactId);
    Task<bool> ContactExistsAsync(Guid userId, Guid contactId);
} 