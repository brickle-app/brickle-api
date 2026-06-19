using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Application.Queries.UserLeasingAgreement;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserLeasingAgreement;

public class GetUserLeasingAgreementByIdQueryHandler : IRequestHandler<GetUserLeasingAgreementByIdQuery, UserLeasingAgreementDto>
{
    private readonly IUserLeasingAgreementRepository _agreementRepository;
    private readonly ICampaignRepository _campaignRepository;

    public GetUserLeasingAgreementByIdQueryHandler(
        IUserLeasingAgreementRepository agreementRepository,
        ICampaignRepository campaignRepository)
    {
        _agreementRepository = agreementRepository;
        _campaignRepository = campaignRepository;
    }

    public async Task<UserLeasingAgreementDto> Handle(GetUserLeasingAgreementByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.UserLeasingAgreement? agreement = await _agreementRepository.GetByIdAsync(request.Id);
        if (agreement == null)
            throw new ApplicationException($"Acuerdo de leasing con id {request.Id} no encontrado");

        var campaign = await _campaignRepository.GetByLeasingIdAsync(agreement.LeasingId);

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