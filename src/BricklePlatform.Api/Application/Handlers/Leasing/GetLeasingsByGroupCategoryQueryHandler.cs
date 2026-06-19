using BricklePlatform.Api.Application.Queries.Leasing;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Enums;
using BricklePlatform.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Api.Application.Handlers.Leasing;

public class GetLeasingsByGroupCategoryQueryHandler : IRequestHandler<GetLeasingsByGroupCategoryQuery, IEnumerable<LeasingDto>>
{
    private readonly ILeasingRepository _leasingRepository;
    private readonly IUserLeasingAgreementRepository _agreementRepository;

    public GetLeasingsByGroupCategoryQueryHandler(ILeasingRepository leasingRepository, IUserLeasingAgreementRepository agreementRepository)
    {
        _leasingRepository = leasingRepository;
        _agreementRepository = agreementRepository;
    }

    public async Task<IEnumerable<LeasingDto>> Handle(GetLeasingsByGroupCategoryQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Leasing> query = _leasingRepository.GetQueryable();

        if (request.Active.HasValue)
        {
            query = query.Where(l => l.Active == request.Active.Value);
        }

        query = request.GroupCategory switch
        {
            LeasingGroupCategoryEnum.LatestSold => query.OrderByDescending(l => l.CreatedAt).Take(10),
            LeasingGroupCategoryEnum.Trending => query.OrderByDescending(l => l.TokensAvailable), // Por ahora, usamos TokensAvailable como proxy para trending
            LeasingGroupCategoryEnum.Recommended => query.OrderByDescending(l => l.Price), // Por ahora, usamos Price como proxy para recommended
            _ => throw new ArgumentException($"Categoría de grupo no soportada: {request.GroupCategory}")
        };

        List<Domain.Entities.Leasing> leasings = await query.ToListAsync(cancellationToken);

        var leasingDtos = new List<LeasingDto>();
        foreach (var leasing in leasings)
        {
            var agreement = await _agreementRepository.GetByLeasingIdAsync(leasing.Id);

            leasingDtos.Add(new LeasingDto
            {
                Id = leasing.Id,
                Name = leasing.Name,
                Quantity = leasing.Quantity,
                Price = leasing.Price,
                Tokens = leasing.Tokens,
                TokensAvailable = leasing.TokensAvailable,
                PricePerToken = leasing.PricePerToken,
                Description = leasing.Description,
                Type = leasing.Type.ToString(),
                ContractTime = leasing.ContractTime,
                Liquidity = leasing.Liquidity.ToString(),
                CoverImageUrl = leasing.CoverImageUrl,
                MiniatureImageUrl = leasing.MiniatureImageUrl,
                DiscoverImageUrl = leasing.DiscoverImageUrl,
                ContractAddress = leasing.ContractAddress,
                TIR = leasing.TIR,
                ReteIcaPct = leasing.ReteIcaPct,
                ReteFuentePct = leasing.ReteFuentePct,
                Active = leasing.Active,
                CreatedAt = leasing.CreatedAt,
                UpdatedAt = leasing.UpdatedAt,
                Agreement = agreement != null ? new UserLeasingAgreementInfoDto
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
                } : null
            });
        }

        return leasingDtos;
    }
}