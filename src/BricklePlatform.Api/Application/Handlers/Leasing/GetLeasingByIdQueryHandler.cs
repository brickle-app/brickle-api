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
    private readonly ICompanyRepository _companyRepository;

    public GetLeasingByIdQueryHandler(
        ILeasingRepository leasingRepository,
        IUserLeasingAgreementRepository agreementRepository,
        ICompanyRepository companyRepository)
    {
        _leasingRepository = leasingRepository;
        _agreementRepository = agreementRepository;
        _companyRepository = companyRepository;
    }

    public async Task<LeasingDto> Handle(GetLeasingByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(request.Id);
        if (leasing == null)
            throw new NotFoundException($"Leasing con id {request.Id} no encontrado");

        var agreement = await _agreementRepository.GetByLeasingIdAsync(leasing.Id);

        // La empresa asignada explícitamente al leasing (desde el admin) tiene prioridad; si no hay,
        // se usa la empresa del arrendatario del acuerdo, como en los leasings creados antes de CompanyId.
        var company = leasing.CompanyId.HasValue
            ? await _companyRepository.GetByIdAsync(leasing.CompanyId.Value)
            : null;
        company ??= agreement?.User?.Company;

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
            CompanyId = leasing.CompanyId,
            Active = leasing.Active,
            CreatedAt = leasing.CreatedAt,
            UpdatedAt = leasing.UpdatedAt,
            Details = leasing.Details?.Select(d => new AssetDetailDto
            {
                Title = d.Title,
                Value = d.Value
            }).ToList(),
            Company = company != null ? new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                OperationTime = company.OperationTime,
                OperationMeasure = company.OperationMeasure,
                CreditRating = company.CreditRating,
                LeasingContract = company.LeasingContract,
                UserId = company.UserId,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
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