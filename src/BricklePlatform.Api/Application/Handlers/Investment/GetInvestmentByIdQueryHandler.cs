using BricklePlatform.Api.Application.Queries.Investment;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Investment
{
    public class GetInvestmentByIdQueryHandler : IRequestHandler<GetInvestmentByIdQuery, InvestmentDto?>
    {
        private readonly IInvestmentRepository _investmentRepository;
        private readonly IUserLeasingAgreementRepository _agreementRepository;
        private readonly ILogger<GetInvestmentByIdQueryHandler> _logger;

        public GetInvestmentByIdQueryHandler(
            IInvestmentRepository investmentRepository,
            IUserLeasingAgreementRepository agreementRepository,
            ILogger<GetInvestmentByIdQueryHandler> logger)
        {
            _investmentRepository = investmentRepository;
            _agreementRepository = agreementRepository;
            _logger = logger;
        }

        public async Task<InvestmentDto?> Handle(GetInvestmentByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo inversión con ID: {InvestmentId}", request.Id);

            var investment = await _investmentRepository.GetByIdAsync(request.Id);

            if (investment == null)
            {
                _logger.LogWarning("No se encontró inversión con ID: {InvestmentId}", request.Id);
                return null;
            }

            var agreement = await _agreementRepository.GetByLeasingIdAsync(investment.LeasingId);

            // Convert entity to DTO
            var investmentDto = new InvestmentDto
            {
                Id = investment.Id,
                UserId = investment.UserId,
                LeasingId = investment.LeasingId,
                Amount = investment.Amount,
                BricksCount = investment.BricksCount,
                BricksName = investment.BricksName,
                PaymentCount = investment.PaymentCount,
                CreatedAt = investment.CreatedAt,
                UpdatedAt = investment.UpdatedAt,
                Leasing = investment.Leasing != null ? new LeasingDto
                {
                    Id = investment.Leasing.Id,
                    Name = investment.Leasing.Name,
                    Quantity = investment.Leasing.Quantity,
                    Price = investment.Leasing.Price,
                    Tokens = investment.Leasing.Tokens,
                    TokensAvailable = investment.Leasing.TokensAvailable,
                    PricePerToken = investment.Leasing.PricePerToken,
                    Description = investment.Leasing.Description,
                    Type = investment.Leasing.Type.ToString(),
                    ContractTime = investment.Leasing.ContractTime,
                    Liquidity = investment.Leasing.Liquidity.ToString(),
                    CoverImageUrl = investment.Leasing.CoverImageUrl,
                    MiniatureImageUrl = investment.Leasing.MiniatureImageUrl,
                    DiscoverImageUrl = investment.Leasing.DiscoverImageUrl,
                    ContractAddress = investment.Leasing.ContractAddress,
                    TIR = investment.Leasing.TIR,
                    ReteIcaPct = investment.Leasing.ReteIcaPct,
                    ReteFuentePct = investment.Leasing.ReteFuentePct,
                    Active = investment.Leasing.Active,
                    CreatedAt = investment.Leasing.CreatedAt,
                    UpdatedAt = investment.Leasing.UpdatedAt,
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
                } : null
            };

            _logger.LogInformation("Inversión con ID: {InvestmentId} obtenida exitosamente", request.Id);

            return investmentDto;
        }
    }
}