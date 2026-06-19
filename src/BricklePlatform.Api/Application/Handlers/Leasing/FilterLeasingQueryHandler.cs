using BricklePlatform.Api.Application.Models;
using BricklePlatform.Api.Application.Queries.Leasing;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Leasing;

public class FilterLeasingQueryHandler : IRequestHandler<FilterLeasingQuery, PaginatedResult<LeasingDto>>
{
    private readonly ILeasingRepository _leasingRepository;
    private readonly IUserLeasingAgreementRepository _agreementRepository;

    public FilterLeasingQueryHandler(ILeasingRepository leasingRepository, IUserLeasingAgreementRepository agreementRepository)
    {
        _leasingRepository = leasingRepository;
        _agreementRepository = agreementRepository;
    }

    public async Task<PaginatedResult<LeasingDto>> Handle(FilterLeasingQuery request, CancellationToken cancellationToken)
    {
        (IEnumerable<Domain.Entities.Leasing> items, int totalCount) = await _leasingRepository.GetFilteredAsync(
            request.Categories,
            request.Page,
            request.Limit,
            request.Active);

        int totalPages = (int)Math.Ceiling(totalCount / (double)request.Limit);

        var leasingDtos = new List<LeasingDto>();
        foreach (var leasing in items)
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

        return new PaginatedResult<LeasingDto>(
            leasingDtos,
            request.Page,
            totalPages,
            totalCount);
    }
}