using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.Interfaces;

public interface ILeasingRepository
{
    IQueryable<Leasing> GetQueryable();

    Task<IEnumerable<Leasing>> GetAllAsync();

    Task<IEnumerable<Leasing>> GetAllWithAgreementsAsync();

    Task<Leasing?> GetByIdAsync(Guid id);

    Task<Leasing?> GetByIdWithAgreementAsync(Guid id);

    Task<(IEnumerable<Leasing> Items, int TotalCount)> GetFilteredAsync(
        IEnumerable<LeasingTypeEnum>? categories,
        int page,
        int limit,
        bool? active = null);

    Task<Leasing> CreateAsync(Leasing leasing);

    Task<Leasing> UpdateAsync(Leasing leasing);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<Leasing>> GetAllActiveAsync(bool Active);
}