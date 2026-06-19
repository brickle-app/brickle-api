using BricklePlatform.Api.Application.Queries.Leasing;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Exceptions;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Leasing;

public class GetLeasingByIdQueryHandler : IRequestHandler<GetLeasingByIdQuery, LeasingDto>
{
    private readonly ILeasingRepository _leasingRepository;
    private readonly IUserLeasingAgreementRepository _agreementRepository;

    public GetLeasingByIdQueryHandler(ILeasingRepository leasingRepository, IUserLeasingAgreementRepository agreementRepository)
    {
        _leasingRepository = leasingRepository;
        _agreementRepository = agreementRepository;
    }

    public async Task<LeasingDto> Handle(GetLeasingByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(request.Id);
        if (leasing == null)
            throw new NotFoundException($"Leasing con id {request.Id} no encontrado");

        var agreement = await _agreementRepository.GetByLeasingIdAsync(leasing.Id);

        // Debug logging to understand the state
        if (agreement != null)
        {
            Console.WriteLine($"Agreement found: UserId={agreement.UserId}");
            Console.WriteLine($"User loaded: {agreement.User != null}");
            if (agreement.User != null)
            {
                Console.WriteLine($"User Company loaded: {agreement.User.Company != null}");
                if (agreement.User.Company != null)
                {
                    Console.WriteLine($"Company ID: {agreement.User.Company.Id}, Name: {agreement.User.Company.Name}");
                }
            }
        }
        else
        {
            Console.WriteLine("No agreement found for this leasing");
        }

        return new LeasingDto
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
            Details = leasing.Details?.Select(d => new AssetDetailDto
            {
                Title = d.Title,
                Value = d.Value
            }).ToList(),
            Company = agreement?.User?.Company != null ? new CompanyDto
            {
                Id = agreement.User.Company.Id,
                Name = agreement.User.Company.Name,
                OperationTime = agreement.User.Company.OperationTime,
                OperationMeasure = agreement.User.Company.OperationMeasure,
                CreditRating = agreement.User.Company.CreditRating,
                LeasingContract = agreement.User.Company.LeasingContract,
                UserId = agreement.User.Company.UserId,
                CreatedAt = agreement.User.Company.CreatedAt,
                UpdatedAt = agreement.User.Company.UpdatedAt
            } : null,
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
        };
    }
}