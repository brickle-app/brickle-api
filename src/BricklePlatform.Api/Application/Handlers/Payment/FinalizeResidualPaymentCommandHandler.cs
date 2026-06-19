using BricklePlatform.Api.Application.Commands.Payment;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace BricklePlatform.Api.Application.Handlers.Payment;

/// <summary>
/// Orquesta el pago residual: valida estado del contrato, comprueba saldo del LeasingCore en token base,
/// envía <c>makeLastLeasingPayment</c> firmado con <c>WalletPrivateKey</c> (gas). Acumula residual + incentivo final en <c>totalClaimableByUser</c> para <c>claimEarnings</c> (mismo flujo que cuotas).
/// Notifica a inversores para que reclamen.
/// </summary>
public class FinalizeResidualPaymentCommandHandler : IRequestHandler<FinalizeResidualPaymentCommand, CreatePaymentResponse>
{
    private readonly IUserLeasingAgreementRepository _agreementRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILeasingRepository _leasingRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IInvestmentRepository _investmentRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogRepository _logRepository;
    private readonly ILeasingCoreService _leasingCoreService;
    private readonly ILogger<FinalizeResidualPaymentCommandHandler> _logger;
    private readonly string _walletPrivateKey;
    private readonly string _fallbackBaseToken;

    public FinalizeResidualPaymentCommandHandler(
        IUserLeasingAgreementRepository agreementRepository,
        IUserRepository userRepository,
        ILeasingRepository leasingRepository,
        ICampaignRepository campaignRepository,
        IInvestmentRepository investmentRepository,
        INotificationService notificationService,
        ILogRepository logRepository,
        ILeasingCoreService leasingCoreService,
        ILogger<FinalizeResidualPaymentCommandHandler> logger,
        IOptions<InfrastructureSettings> settings)
    {
        _agreementRepository = agreementRepository;
        _userRepository = userRepository;
        _leasingRepository = leasingRepository;
        _campaignRepository = campaignRepository;
        _investmentRepository = investmentRepository;
        _notificationService = notificationService;
        _logRepository = logRepository;
        _leasingCoreService = leasingCoreService;
        _logger = logger;
        _walletPrivateKey = settings.Value.Web3Settings.WalletPrivateKey ?? string.Empty;
        _fallbackBaseToken = settings.Value.Web3Settings.BASE_TOKEN ?? string.Empty;
    }

    public async Task<CreatePaymentResponse> Handle(FinalizeResidualPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalize residual payment for agreement {UserLeasingAgreementId}", request.Body.UserLeasingAgreementId);

        if (string.IsNullOrWhiteSpace(_walletPrivateKey))
        {
            _logger.LogWarning("WalletPrivateKey no configurada; no se puede ejecutar makeLastLeasingPayment");
            return new CreatePaymentResponse(false, string.Empty, 0m, 0m,
                "Configure InfrastructureSettings:Web3Settings:WalletPrivateKey para firmar el pago residual en servidor.");
        }

        var agreement = await _agreementRepository.GetByIdAsync(request.Body.UserLeasingAgreementId);
        if (agreement == null)
            throw new ApplicationException($"Contrato de arrendamiento de usuario con Id: {request.Body.UserLeasingAgreementId} no encontrado");

        var user = await _userRepository.GetByIdAsync(agreement.UserId);
        if (user == null)
            throw new ApplicationException($"Usuario con ID {agreement.UserId} no encontrado");

        var leasing = await _leasingRepository.GetByIdAsync(agreement.LeasingId);
        if (leasing == null)
            throw new ApplicationException($"Leasing con ID {agreement.LeasingId} no encontrado");

        if (string.IsNullOrWhiteSpace(agreement.LeasingCoreAddress))
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "El acuerdo no tiene LeasingCore configurado.");

        var expectedPayment = await _leasingCoreService.GetExpectedPaymentAsync(agreement.LeasingCoreAddress);
        if (expectedPayment == null || !expectedPayment.IsResidualPayment)
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "No hay pago residual pendiente en el contrato (o ya se completó).");

        var state = await _leasingCoreService.GetLeasingContractStateAsync(agreement.LeasingCoreAddress);
        if (state == null || !state.IsResidualPayment || state.LastPaymentMade)
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "El contrato no está en estado de pago residual pendiente.");

        if (!state.LeasingTokenTotalSupply.HasValue)
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "No se pudo leer totalSupply del LeasingToken (participación). Revise RPC y la dirección del Core.");

        if (state.LeasingTokenTotalSupply.Value == 0)
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "El LeasingToken tiene totalSupply 0: el contrato no puede repartir residual + incentivo (revertiría con «No leasing token supply»). " +
                "El saldo del token base (COP) dentro del LeasingCore es otro dato: puede ser alto aunque el supply del token de participación sea 0 tras quemas. " +
                "Cierre residual debe ejecutarse con supply > 0 o cambiar el protocolo/contrato.");

        if (state.ResidualValue is null || state.FinalPaymentAmount is null)
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "No se pudo leer residualValue o finalPaymentAmount del contrato.");

        var totalNeeded = state.ResidualValue.Value + state.FinalPaymentAmount.Value;

        string? tokenFromContract = await _leasingCoreService.GetBaseTokenAsync(agreement.LeasingCoreAddress);
        var campaign = await _campaignRepository.GetByLeasingIdAsync(agreement.LeasingId);
        string tokenAddress = !string.IsNullOrWhiteSpace(tokenFromContract)
            ? tokenFromContract
            : !string.IsNullOrWhiteSpace(campaign?.BaseToken)
                ? campaign.BaseToken
                : _fallbackBaseToken;

        if (string.IsNullOrWhiteSpace(tokenAddress))
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "No se pudo determinar el token base del LeasingCore.");

        var coreBalance = await _leasingCoreService.GetErc20BalanceAsync(tokenAddress, agreement.LeasingCoreAddress);
        if (!coreBalance.HasValue)
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "No se pudo leer balanceOf del token base en el LeasingCore (RPC o dirección de token). " +
                "Revise que baseToken() del contrato y la red coincidan con la configuración del API.");

        if (coreBalance.Value < totalNeeded)
        {
            var needHuman = (decimal)totalNeeded / (decimal)Math.Pow(10, 6);
            var haveHuman = (decimal)coreBalance.Value / (decimal)Math.Pow(10, 6);
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                $"Saldo insuficiente en LeasingCore en el token base. Necesario ~{needHuman:N2} (residual + finalPayment), disponible ~{haveHuman:N2}. " +
                "El comprador debe haber enviado suficientes pagos mensuales para que el contrato retenga residual e incentivo; si los inversores reclamaron de más, el saldo puede quedar corto.");
        }

        string clientAddress = !string.IsNullOrWhiteSpace(request.Body.ClientAddress)
            ? request.Body.ClientAddress.Trim()
            : user.WalletAddress?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(clientAddress) || !clientAddress.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || clientAddress.Length != 42)
            return new CreatePaymentResponse(false, string.Empty, 0m, agreement.RemainingBalance,
                "Dirección de cliente inválida. Indique ClientAddress o asocie WalletAddress al usuario del acuerdo.");

        decimal amountHuman = (decimal)expectedPayment.Amount / (decimal)Math.Pow(10, 6);
        string txHash;

        try
        {
            txHash = await _leasingCoreService.SendMakeLastLeasingPaymentAsync(
                _walletPrivateKey,
                agreement.LeasingCoreAddress,
                clientAddress,
                expectedPayment.Amount,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando makeLastLeasingPayment para acuerdo {AgreementId}", agreement.Id);
            await InsertLogAsync(agreement, leasing, user, string.Empty, false, amountHuman);
            return new CreatePaymentResponse(false, string.Empty, amountHuman, agreement.RemainingBalance,
                $"Error en blockchain: {ex.Message}");
        }

        _logger.LogInformation("makeLastLeasingPayment OK. Tx: {TxHash}, Agreement: {AgreementId}", txHash, agreement.Id);

        await NotifyInvestorsResidualAsync(agreement, leasing, amountHuman);

        var newRemainingBalance = agreement.RemainingBalance;
        await InsertLogAsync(agreement, leasing, user, txHash, true, amountHuman, newRemainingBalance);

        return new CreatePaymentResponse(true, txHash, amountHuman, newRemainingBalance, null);
    }

    private async Task NotifyInvestorsResidualAsync(Domain.Entities.UserLeasingAgreement agreement, Domain.Entities.Leasing leasing, decimal amountHuman)
    {
        try
        {
            var investors = await _investmentRepository.GetInvestorsByLeasingIdAsync(agreement.LeasingId);
            var investorsWithTokens = investors.Where(u => !string.IsNullOrEmpty(u.PushNotificationToken)).ToList();
            if (investorsWithTokens.Count == 0)
            {
                _logger.LogInformation("Sin inversores con push para notificar tras pago residual: {LeasingId}", agreement.LeasingId);
                return;
            }

            var notifications = investorsWithTokens.Select(investor => new NotificationDto
            {
                RecipientId = investor.PushNotificationToken!,
                Title = "Leasing finalizado (paso residual)",
                Body =
                    $"Se ejecutó el cierre de {leasing.Name}: residual e incentivo final ya están en tu saldo reclamable en la app (mismo reclamo que las cuotas).",
                Data = new Dictionary<string, object>
                {
                    { "type", "INVESTMENT-RETURN" },
                    { "leasingId", agreement.LeasingId.ToString() },
                    { "paymentAmount", amountHuman.ToString() },
                    { "isLastPayment", true }
                }
            });

            await _notificationService.SendBulkNotificationsAsync(notifications);
            _logger.LogInformation("Notificaciones de pago residual enviadas a {Count} inversores", investorsWithTokens.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificaciones tras pago residual: {LeasingId}", agreement.LeasingId);
        }
    }

    private async Task InsertLogAsync(
        Domain.Entities.UserLeasingAgreement agreement,
        Domain.Entities.Leasing leasing,
        Domain.Entities.User user,
        string hash,
        bool status,
        decimal amountHuman,
        decimal? remainingOverride = null)
    {
        var remaining = remainingOverride ?? agreement.RemainingBalance;
        var paymentLog = new PaymentLogDto
        {
            UserLeasingAgreementId = agreement.Id,
            PaymentAmount = amountHuman,
            TotalValue = agreement.TotalValue,
            RemainingBalance = remaining,
            LeasingContractAddress = leasing.ContractAddress,
            UserWallet = user.WalletAddress ?? string.Empty,
            Hash = hash,
            Status = status
        };
        await _logRepository.InsertPaymentLogAsync(paymentLog);
    }
}
