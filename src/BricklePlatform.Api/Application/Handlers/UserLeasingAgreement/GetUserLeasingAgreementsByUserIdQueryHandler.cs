using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Application.Queries.UserLeasingAgreement;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserLeasingAgreement;

public class GetUserLeasingAgreementsByUserIdQueryHandler : IRequestHandler<GetUserLeasingAgreementsByUserIdQuery, IEnumerable<UserLeasingAgreementDto>>
{
    private readonly IUserLeasingAgreementRepository _agreementRepository;
    private readonly ICampaignRepository _campaignRepository;

    public GetUserLeasingAgreementsByUserIdQueryHandler(
        IUserLeasingAgreementRepository agreementRepository,
        ICampaignRepository campaignRepository)
    {
        _agreementRepository = agreementRepository;
        _campaignRepository = campaignRepository;
    }

    public async Task<IEnumerable<UserLeasingAgreementDto>> Handle(GetUserLeasingAgreementsByUserIdQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.UserLeasingAgreement> agreements = await _agreementRepository.GetByUserIdAsync(request.UserId);

        if (!agreements.Any())
            throw new ApplicationException($"No se encontraron acuerdos de leasing para el usuario {request.UserId}");

        var result = new List<UserLeasingAgreementDto>();
        foreach (var agreement in agreements)
        {
            var campaign = await _campaignRepository.GetByLeasingIdAsync(agreement.LeasingId);
            result.Add(new UserLeasingAgreementDto
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
                LeasingCoreAddress = agreement.LeasingCoreAddress,
                BaseToken = campaign?.BaseToken,
                Status = agreement.Status,
                ReteIcaPct = agreement.ReteIcaPct,
                ReteFuentePct = agreement.ReteFuentePct,
                BuyerRetentionPercentage = agreement.BuyerRetentionPercentage,
                CreatedAt = agreement.CreatedAt,
                UpdatedAt = agreement.UpdatedAt
            });
        }
        return result;
    }
}