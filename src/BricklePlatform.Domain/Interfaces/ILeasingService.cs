using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface ILeasingService
{
    Task<Leasing> CreateLeasingAsync(CreateLeasingDto leasingDto, User createdBy);
    Task<Leasing?> GetLeasingAsync(Guid id);
    Task<IEnumerable<Leasing>> GetAllLeasingsAsync();
    Task<Leasing> UpdateLeasingAsync(Guid id, UpdateLeasingDto leasingDto, User updatedBy);
    Task DeleteLeasingAsync(Guid id);
}