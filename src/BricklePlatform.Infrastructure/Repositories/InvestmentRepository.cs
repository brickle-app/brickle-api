using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories
{
    public class InvestmentRepository : IInvestmentRepository
    {
        private readonly ApplicationDbContext _context;

        public InvestmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private static UserLeasingAgreementInfoDto? ToAgreementInfoDto(UserLeasingAgreement? agreement)
        {
            if (agreement == null) return null;
            return new UserLeasingAgreementInfoDto
            {
                Id = agreement.Id,
                UserId = agreement.UserId,
                LeasingId = agreement.LeasingId,
                AssetValue = agreement.AssetValue,
                UsefulLife = agreement.UsefulLife,
                TermTime = agreement.TermTime,
                PaymentTerm = agreement.PaymentTerm,
                AgreementType = agreement.AgreementType,
                Currency = agreement.Currency,
                ContractDetails = agreement.ContractDetails,
                StartDate = agreement.StartDate,
                EndDate = agreement.EndDate,
                InstallmentRate = agreement.InstallmentRate,
                InstallmentAmount = agreement.InstallmentAmount,
                ManagementFee = agreement.ManagementFee,
                TotalValue = agreement.TotalValue,
                RemainingBalance = agreement.RemainingBalance,
                Status = agreement.Status,
                LeasingCoreAddress = agreement.LeasingCoreAddress,
                InsurancePercentage = agreement.InsurancePercentage,
                IbrRate = agreement.IbrRate,
                RiskLevel = agreement.RiskLevel,
                RiskRate = agreement.RiskRate,
                IVA = agreement.IVA,
                ReteIcaPct = agreement.ReteIcaPct,
                ReteFuentePct = agreement.ReteFuentePct,
                BuyerRetentionPercentage = agreement.BuyerRetentionPercentage,
                CreatedAt = agreement.CreatedAt,
                UpdatedAt = agreement.UpdatedAt
            };
        }

        private static LeasingDto ToLeasingDto(Leasing l, UserLeasingAgreementInfoDto? agreement)
        {
            return new LeasingDto
            {
                Id = l.Id,
                Name = l.Name,
                Quantity = l.Quantity,
                Price = l.Price,
                Tokens = l.Tokens,
                TokensAvailable = l.TokensAvailable,
                PricePerToken = l.PricePerToken,
                Description = l.Description,
                Type = l.Type.ToString(),
                ContractTime = l.ContractTime,
                Liquidity = l.Liquidity.ToString(),
                CoverImageUrl = l.CoverImageUrl,
                MiniatureImageUrl = l.MiniatureImageUrl,
                DiscoverImageUrl = l.DiscoverImageUrl,
                ContractAddress = l.ContractAddress,
                TIR = l.TIR,
                ReteIcaPct = l.ReteIcaPct,
                ReteFuentePct = l.ReteFuentePct,
                Active = l.Active,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                Agreement = agreement
            };
        }

        private static InvestmentDto ToInvestmentDto(Investment i, UserLeasingAgreement? agreementEntity)
        {
            if (i.Leasing == null)
                throw new InvalidOperationException("Investment.Leasing must be loaded");

            return new InvestmentDto
            {
                Id = i.Id,
                UserId = i.UserId,
                LeasingId = i.LeasingId,
                Amount = i.Amount,
                BricksCount = i.BricksCount,
                BricksName = i.BricksName,
                PaymentCount = i.PaymentCount,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                Leasing = ToLeasingDto(i.Leasing, ToAgreementInfoDto(agreementEntity))
            };
        }

        private async Task<Dictionary<Guid, UserLeasingAgreement>> LoadAgreementFirstByLeasingIdsAsync(
            IEnumerable<Guid> leasingIds)
        {
            var ids = leasingIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, UserLeasingAgreement>();

            var rows = await _context.UserLeasingAgreements
                .AsNoTracking()
                .Where(a => ids.Contains(a.LeasingId))
                .ToListAsync();

            return rows
                .GroupBy(a => a.LeasingId)
                .ToDictionary(g => g.Key, g => g.First());
        }

        public async Task<Investment> CreateAsync(Investment investment)
        {
            _context.Investments.Add(investment);
            await _context.SaveChangesAsync();
            return investment;
        }

        public async Task<Investment?> GetByIdAsync(Guid id)
        {
            return await _context.Investments
                .Include(i => i.Leasing)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Investment?> GetByUserIdAndLeasingIdAsync(Guid userId, Guid leasingId)
        {
            return await _context.Investments
                .Include(i => i.Leasing)
                .FirstOrDefaultAsync(i => i.UserId == userId && i.LeasingId == leasingId);
        }

        public async Task<IEnumerable<InvestmentDto>> GetByUserIdAsync(Guid userId)
        {
            var investments = await _context.Investments
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .Include(i => i.Leasing)
                .ToListAsync();

            var agreementMap = await LoadAgreementFirstByLeasingIdsAsync(investments.Select(i => i.LeasingId));

            return investments
                .Select(i => ToInvestmentDto(i, agreementMap.GetValueOrDefault(i.LeasingId)))
                .ToList();
        }

        public async Task<IEnumerable<InvestmentDto>> GetByLeasingIdAsync(Guid leasingId)
        {
            var investments = await _context.Investments
                .AsNoTracking()
                .Where(i => i.LeasingId == leasingId)
                .Include(i => i.Leasing)
                .Include(i => i.User)
                .ToListAsync();

            var agreement = await _context.UserLeasingAgreements
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.LeasingId == leasingId);

            return investments.Select(i => ToInvestmentDto(i, agreement)).ToList();
        }

        public async Task<IEnumerable<User>> GetInvestorsByLeasingIdAsync(Guid leasingId)
        {
            return await _context.Investments
                .Where(i => i.LeasingId == leasingId)
                .Include(i => i.User)
                .Select(i => i.User)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<InvestmentDto>> GetAllAsync()
        {
            var investments = await _context.Investments
                .AsNoTracking()
                .Include(i => i.Leasing)
                .ToListAsync();

            var agreementMap = await LoadAgreementFirstByLeasingIdsAsync(investments.Select(i => i.LeasingId));

            return investments
                .Select(i => ToInvestmentDto(i, agreementMap.GetValueOrDefault(i.LeasingId)))
                .ToList();
        }

        public async Task<Investment> UpdateAsync(Investment investment)
        {
            _context.Investments.Update(investment);
            await _context.SaveChangesAsync();
            return investment;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var investment = await _context.Investments.FindAsync(id);
            if (investment == null)
                return false;

            _context.Investments.Remove(investment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}