using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces
{
    public interface IUserLeasingAgreementRepository
    {
        Task<UserLeasingAgreement?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserLeasingAgreement>> GetByUserIdAsync(Guid userId);
        Task<UserLeasingAgreement?> GetByLeasingIdAsync(Guid leasingId);
        Task<IEnumerable<UserLeasingAgreement>> GetAllByLeasingIdAsync(Guid leasingId);
        Task<UserLeasingAgreement> AddAsync(UserLeasingAgreement agreement);
        Task<UserLeasingAgreement> UpdateAsync(UserLeasingAgreement agreement);
        Task<bool> UpdateRemainingBalanceAsync(Guid id, decimal remainingBalance);
        Task<bool> ProcessPaymentAsync(Guid id, decimal paymentAmount);
        Task<bool> DeleteAsync(Guid id);
    }
}