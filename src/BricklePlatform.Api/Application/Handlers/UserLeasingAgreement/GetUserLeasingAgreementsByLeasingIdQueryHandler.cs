using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Application.Queries.UserLeasingAgreement;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserLeasingAgreement;

public class GetUserLeasingAgreementsByLeasingIdQueryHandler : IRequestHandler<GetUserLeasingAgreementsByLeasingIdQuery, UserLeasingAgreementDto>
{
    private readonly IUserLeasingAgreementRepository _agreementRepository;
    private readonly ICampaignRepository _campaignRepository;

    public GetUserLeasingAgreementsByLeasingIdQueryHandler(
        IUserLeasingAgreementRepository agreementRepository,
        ICampaignRepository campaignRepository)
    {
        _agreementRepository = agreementRepository;
        _campaignRepository = campaignRepository;
    }

    public async Task<UserLeasingAgreementDto> Handle(GetUserLeasingAgreementsByLeasingIdQuery request, CancellationToken cancellationToken)
    {
        var agreement = await _agreementRepository.GetByLeasingIdAsync(request.LeasingId);

        if (agreement == null)
            throw new ApplicationException($"No se encontraron acuerdos de leasing para el leasingId {request.LeasingId}");

        var campaign = await _campaignRepository.GetByLeasingIdAsync(request.LeasingId);

        return new UserLeasingAgreementDto
        {
            Id = agreement.Id,
            UserId = agreement.UserId,
            LeasingId = agreement.LeasingId,
            PaymentTerm = agreement.PaymentTerm,
            Currency = agreement.Currency,
            ContractDetails = agreement.ContractDetails,
            StartDate = agreement.StartDate,
            EndDate = agreement.EndDate,
            InstallmentAmount = agreement.InstallmentAmount,
            TotalValue = agreement.TotalValue,
            RemainingBalance = agreement.RemainingBalance,
            Status = agreement.Status,
            LeasingCoreAddress = agreement.LeasingCoreAddress,
            BaseToken = campaign?.BaseToken,
            ReteIcaPct = agreement.ReteIcaPct,
            ReteFuentePct = agreement.ReteFuentePct,
            BuyerRetentionPercentage = agreement.BuyerRetentionPercentage,
            CreatedAt = agreement.CreatedAt,
            UpdatedAt = agreement.UpdatedAt
        };
    }
}