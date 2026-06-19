using BricklePlatform.Api.Application.Commands.Investment;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Investment;

public class ClaimRentCommandHandler : IRequestHandler<ClaimRentCommand, bool>
{
    private readonly IWebHookService _webHookService;
    private readonly ILeasingRepository _leasingRepository;
    private readonly IInvestmentRepository _investmentRepository;
    private readonly IUserLeasingAgreementRepository _agreementRepository;
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly ILeasingCoreService _leasingCoreService;
    private readonly ILogger<ClaimRentCommandHandler> _logger;

    public ClaimRentCommandHandler(
        IWebHookService webHookService,
        ILeasingRepository leasingRepository,
        IInvestmentRepository investmentRepository,
        IUserLeasingAgreementRepository agreementRepository,
        IUserActivityLogService userActivityLogService,
        ILeasingCoreService leasingCoreService,
        ILogger<ClaimRentCommandHandler> logger)
    {
        _webHookService = webHookService;
        _leasingRepository = leasingRepository;
        _investmentRepository = investmentRepository;
        _agreementRepository = agreementRepository;
        _userActivityLogService = userActivityLogService;
        _leasingCoreService = leasingCoreService;
        _logger = logger;
    }

    public async Task<bool> Handle(ClaimRentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Claiming rent for Leasing: {LeasingId}, User: {UserId} - CorrelationId: {CorrelationId}",
           request.LeasingId, request.UserId, request.Header.CorrelationId);

        short[] vValues = { 27, 28 };
        if (!vValues.Contains(request.ClaimRentDto.PermitSignature.V) ||
           request.ClaimRentDto.PermitSignature.R == null ||
           request.ClaimRentDto.PermitSignature.S == null)
        {
            _logger.LogWarning("Permit signature is incomplete for Leasing: {LeasingId}, User: {UserId} - CorrelationId: {CorrelationId}",
                request.LeasingId, request.UserId, request.Header.CorrelationId);
            throw new ApplicationException("Permit signature is incomplete");
        }

        var leasing = await _leasingRepository.GetByIdAsync(request.LeasingId);

        if (leasing == null)
        {
            _logger.LogWarning("Leasing not found: {LeasingId} - CorrelationId: {CorrelationId}",
                request.LeasingId, request.Header.CorrelationId);
            throw new ApplicationException("Leasing not found");
        }

        WebhookResponseDto webhookResponse = await _webHookService.ProcessClaimRent(
            request.ClaimRentDto.Token,
            leasing.ContractAddress,
            request.ClaimRentDto.Receiver,
            100000,
            request.ClaimRentDto.Deadline,
            request.ClaimRentDto.PermitSignature);

        if (webhookResponse.Status)
        {
            var investment = await _investmentRepository.GetByUserIdAndLeasingIdAsync(request.UserId, request.LeasingId);

            if (investment != null)
            {
                var previousPaymentCount = investment.PaymentCount;
                int installmentsInClaim = 1;

                var currentMonth = await _leasingCoreService.GetCurrentMonthAsync(leasing.ContractAddress ?? string.Empty);
                if (currentMonth.HasValue)
                {
                    var delta = (int)currentMonth.Value - previousPaymentCount;
                    installmentsInClaim = Math.Max(1, delta);
                }

                investment.IncrementPaymentCountBy(installmentsInClaim);

                var reference = installmentsInClaim > 1
                    ? $"Renta reclamada - Cuotas {previousPaymentCount + 1} a {previousPaymentCount + installmentsInClaim}"
                    : $"Renta reclamada - Cuota {previousPaymentCount + 1}";

                // --- Calcular desglose capital / interés ---
                var agreement = await _agreementRepository.GetByLeasingIdAsync(request.LeasingId);

                var split = CalculateCapitalInterestSplit(
                    investment,
                    agreement,
                    leasing.Tokens,
                    previousPaymentCount,
                    installmentsInClaim,
                    request.ClaimRentDto.Amount);

                _logger.LogInformation(
                    "Rent split — LeasingId={LeasingId} TxAmount={Total} Interest={Interest} Capital={Capital} Installments={N} HasSplit={HasSplit}",
                    request.LeasingId, request.ClaimRentDto.Amount, split.Interest, split.Capital, installmentsInClaim, split.HasSplit);

                if (split.HasSplit && split.Capital > 0 && leasing.PricePerToken > 0)
                {
                    var bricksToBurn = split.Capital / leasing.PricePerToken;
                    _logger.LogInformation("Burning {Bricks} bricks for Leasing {LeasingId} due to capital return of {Capital}", 
                        bricksToBurn, request.LeasingId, split.Capital);
                    investment.DeductBricks(bricksToBurn);
                }

                await _investmentRepository.UpdateAsync(investment);

                await LogRentClaimAsync(
                    request.UserId,
                    request.LeasingId,
                    request.ClaimRentDto.Amount,
                    webhookResponse.Hash,
                    reference,
                    split);
            }

            _logger.LogInformation("Rent claimed successfully for Leasing: {LeasingId}, User: {UserId}, Hash: {Hash} - CorrelationId: {CorrelationId}",
                request.LeasingId, request.UserId, webhookResponse.Hash, request.Header.CorrelationId);
            return true;
        }

        _logger.LogWarning("Failed to claim rent for Leasing: {LeasingId}, User: {UserId} - CorrelationId: {CorrelationId}",
            request.LeasingId, request.UserId, request.Header.CorrelationId);
        return false;
    }

    private record RentSplit(decimal Interest, decimal Capital, bool HasSplit);

    private RentSplit CalculateCapitalInterestSplit(
        Domain.Entities.Investment investment,
        Domain.Entities.UserLeasingAgreement? agreement,
        decimal leasingTotalTokens,
        int previousPaymentCount,
        int installmentsInClaim,
        decimal totalTxAmount)
    {
        if (agreement == null ||
            agreement.InstallmentRate <= 0 ||
            agreement.InstallmentAmount <= 0 ||
            agreement.AssetValue <= 0 ||
            leasingTotalTokens <= 0 ||
            investment.BricksCount <= 0)
        {
            _logger.LogWarning(
                "Cannot split rent (missing agreement data). Falling back to legacy INVESTMENT-RETURN. LeasingId={LeasingId}",
                investment.LeasingId);
            return new RentSplit(Interest: totalTxAmount, Capital: 0m, HasSplit: false);
        }

        var monthlyRate = agreement.InstallmentRate / 100m;
        var canon = agreement.InstallmentAmount;
        var userShare = (decimal)investment.BricksCount / leasingTotalTokens;
        var managementFeeMonthly = agreement.ManagementFee > 0
            ? agreement.ManagementFee / 100m / 12m
            : 0m;

        var currentAssetValue = agreement.AssetValue;
        for (int m = 0; m < previousPaymentCount; m++)
        {
            var grossInterest = currentAssetValue * monthlyRate;
            var capitalPayment = canon - grossInterest;
            currentAssetValue -= capitalPayment;
            if (currentAssetValue < 0) currentAssetValue = 0;
        }

        decimal totalInterestUser = 0m;
        decimal totalCapitalUser = 0m;

        for (int m = 0; m < installmentsInClaim; m++)
        {
            if (currentAssetValue <= 0)
                break;

            var grossInterest = currentAssetValue * monthlyRate;
            var brickleInterest = currentAssetValue * managementFeeMonthly;
            var tokenHolderInterest = grossInterest - brickleInterest;
            var capitalPayment = canon - grossInterest;

            totalInterestUser += tokenHolderInterest * userShare;
            totalCapitalUser += capitalPayment * userShare;

            currentAssetValue -= capitalPayment;
            if (currentAssetValue < 0) currentAssetValue = 0;
        }

        var calculated = totalInterestUser + totalCapitalUser;
        if (calculated <= 0)
            return new RentSplit(Interest: totalTxAmount, Capital: 0m, HasSplit: false);

        // El capital devuelto en el smart contract es íntegro (no tiene retenciones ni impuestos).
        // Las retenciones solo aplican al interés. Por tanto, el capitalFinal es exactamente el simulado.
        var capitalFinal = Math.Round(totalCapitalUser, 2);
        
        // El interés final es el remanente del TxAmount que recibió el usuario on-chain.
        var interestFinal = totalTxAmount - capitalFinal;

        // Salvaguarda (edge case): si on-chain se recibió menos dinero que el capital teórico,
        // limitamos el capital devuelto al monto total recibido para no generar intereses negativos.
        if (interestFinal < 0)
        {
            capitalFinal = totalTxAmount;
            interestFinal = 0m;
        }

        return new RentSplit(Interest: interestFinal, Capital: capitalFinal, HasSplit: true);
    }

    private async Task LogRentClaimAsync(
        Guid userId,
        Guid leasingId,
        decimal totalAmount,
        string txHash,
        string reference,
        RentSplit split)
    {
        if (!split.HasSplit || split.Capital <= 0)
        {
            await _userActivityLogService.LogUserActivityAsync(new UserActivityLogDto
            {
                UserId = userId,
                Type = "INVESTMENT-RETURN",
                TxAmount = totalAmount,
                Status = "SUCCESS",
                Receipt = "",
                Hash = txHash,
                Reference = reference,
                LeasingId = leasingId,
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        // Log 1 — Intereses netos al usuario (suma al balance; no resta al portafolio de capital)
        if (split.Interest > 0)
        {
            await _userActivityLogService.LogUserActivityAsync(new UserActivityLogDto
            {
                UserId = userId,
                Type = "INVESTMENT-RETURN-INTEREST",
                TxAmount = split.Interest,
                Status = "SUCCESS",
                Receipt = "",
                Hash = txHash,
                Reference = $"{reference} [Rendimiento]",
                LeasingId = leasingId,
                Timestamp = DateTime.UtcNow
            });
        }

        // Log 2 — Capital amortizado devuelto (suma al balance; resta al valor del portafolio invertido)
        if (split.Capital > 0)
        {
            await _userActivityLogService.LogUserActivityAsync(new UserActivityLogDto
            {
                UserId = userId,
                Type = "INVESTMENT-RETURN-CAPITAL",
                TxAmount = split.Capital,
                Status = "SUCCESS",
                Receipt = "",
                Hash = txHash,
                Reference = $"{reference} [Capital]",
                LeasingId = leasingId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}