using System.Numerics;
using BricklePlatform.Api.Application.Commands.Payment;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace BricklePlatform.Api.Application.Handlers.Payment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse>
{
    private IUserLeasingAgreementRepository _agreementRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILeasingRepository _leasingRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IInvestmentRepository _investmentRepository;
    private readonly IWebHookService _webHookService;
    private readonly INotificationService _notificationService;
    private readonly ILogRepository _logRepository;
    private readonly ILeasingCoreService _leasingCoreService;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;
    private readonly string _paymentWalletAddress;
    private readonly string _fallbackBaseToken;

    public CreatePaymentCommandHandler(
        IUserLeasingAgreementRepository agreementRepository,
        IUserRepository userRepository,
        ILeasingRepository leasingRepository,
        ICampaignRepository campaignRepository,
        IInvestmentRepository investmentRepository,
        IWebHookService webHookService,
        INotificationService notificationService,
        ILogRepository logRepository,
        ILeasingCoreService leasingCoreService,
        ILogger<CreatePaymentCommandHandler> logger,
        IOptions<InfrastructureSettings> settings)
    {
        _agreementRepository = agreementRepository;
        _userRepository = userRepository;
        _leasingRepository = leasingRepository;
        _campaignRepository = campaignRepository;
        _investmentRepository = investmentRepository;
        _webHookService = webHookService;
        _notificationService = notificationService;
        _logRepository = logRepository;
        _leasingCoreService = leasingCoreService;
        _logger = logger;
        _paymentWalletAddress = settings.Value.Web3Settings.PaymentWalletAddress ?? string.Empty;
        _fallbackBaseToken = settings.Value.Web3Settings.BASE_TOKEN ?? string.Empty;
    }

    public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando procesamiento de pago para el contrato de arrendamiento de usuario: {UserLeasingAgreementId}",
            request.Body.UserLeasingAgreementId);

        Domain.Entities.UserLeasingAgreement? agreement = await _agreementRepository.GetByIdAsync(request.Body.UserLeasingAgreementId);

        if (agreement == null)
        {
            _logger.LogWarning("Contrato de arrendamiento de usuario no encontrtado: {UserLeasingAgreementId}",
                request.Body.UserLeasingAgreementId);
            throw new ApplicationException($"Contrato de arrendamiento de usuario con Id: {request.Body.UserLeasingAgreementId} no encontrado");
        }

        Domain.Entities.User? user = await _userRepository.GetByIdAsync(agreement.UserId);
        if (user == null)
        {
            _logger.LogWarning("Usuario no encontrado con ID: {UserId}",
                agreement.UserId);
            throw new ApplicationException($"Usuario con ID {agreement.UserId} no encontrado");
        }

        Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(agreement.LeasingId);
        if (leasing == null)
        {
            _logger.LogWarning("Leasing no encontrado con ID: {LeasingId}",
                agreement.LeasingId);
            throw new ApplicationException($"Leasing con ID {agreement.LeasingId} no encontrado");
        }

        // Sender debe ser exactamente la wallet que firmó el EIP-2612 permit (Paymaster: permit(sender, paymaster, ...)).
        // Si aquí se usa otra dirección, el token revierte con ERC2612InvalidSigner (OZ ERC20Permit).
        // Prioridad: 1) request.Sender explícito, 2) wallet del usuario del acuerdo (caso normal), 3) PaymentWalletAddress solo si no hay wallet de usuario.
        string senderWallet = !string.IsNullOrWhiteSpace(request.Body.Sender)
            ? request.Body.Sender
            : !string.IsNullOrWhiteSpace(user.WalletAddress)
                ? user.WalletAddress
                : _paymentWalletAddress;

        // Leer baseToken desde LeasingCore en blockchain (fuente de verdad). Evita desincronización
        // cuando Campaign.BaseToken en DB tiene valor antiguo (ej. 0x0dd9... en vez de 0x317D...).
        string? tokenFromContract = await _leasingCoreService.GetBaseTokenAsync(agreement.LeasingCoreAddress ?? string.Empty);
        var campaign = await _campaignRepository.GetByLeasingIdAsync(agreement.LeasingId);
        string tokenAddress = !string.IsNullOrWhiteSpace(tokenFromContract)
            ? tokenFromContract
            : !string.IsNullOrWhiteSpace(campaign?.BaseToken)
                ? campaign.BaseToken
                : _fallbackBaseToken;

        // Validar que el monto coincida con el esperado por el LeasingCore (cuota mensual o residualValue en última cuota)
        var expectedPayment = await _leasingCoreService.GetExpectedPaymentAsync(agreement.LeasingCoreAddress ?? string.Empty);

        if (expectedPayment?.IsResidualPayment == true)
        {
            _logger.LogWarning("Pago residual rechazado vía Paymaster/permit. Acuerdo: {AgreementId}", agreement.Id);
            var residualHuman = (decimal)expectedPayment.Amount / (decimal)Math.Pow(10, 6);
            return new CreatePaymentResponse(
                false,
                string.Empty,
                residualHuman,
                agreement.RemainingBalance,
                "El pago final (valor residual) no usa permit ni Paymaster. Use POST /api/Payment/finalize-residual con el mismo encabezado de API.");
        }

        if (expectedPayment != null && expectedPayment.Amount > 0)
        {
            if (!BigInteger.TryParse(request.Body.PaymentAmount, out var parsedAmount) || parsedAmount != expectedPayment.Amount)
            {
                var expectedStr = expectedPayment.Amount.ToString();
                var paymentType = expectedPayment.IsResidualPayment ? "valor residual" : "cuota mensual";
                _logger.LogWarning("Monto de pago no coincide con el esperado por el contrato. Enviado: {Sent}, Esperado: {Expected} ({PaymentType})",
                    request.Body.PaymentAmount, expectedStr, paymentType);
                return new CreatePaymentResponse(
                    false,
                    string.Empty,
                    decimal.Parse(request.Body.PaymentAmount) / (decimal)Math.Pow(10, 6),
                    agreement.RemainingBalance,
                    $"El monto debe ser exactamente {expectedStr} ({paymentType} del contrato). Enviado: {request.Body.PaymentAmount}. Use el monto sugerido en el formulario de pago.");
            }
        }

        if (!BigInteger.TryParse(request.Body.PaymentAmount, out var paymentAmountBig))
        {
            _logger.LogWarning("Monto de pago inválido (no numérico): {Amount}", request.Body.PaymentAmount);
            return new CreatePaymentResponse(
                false,
                string.Empty,
                0m,
                agreement.RemainingBalance,
                "El monto de pago no es un entero válido en wei del token (6 decimales).");
        }

        // Solo rechazar por saldo si la lectura on-chain tuvo éxito. Antes, un fallo de RPC/decodificación
        // devolvía 0 y bloqueaba el Paymaster con un mensaje de "saldo insuficiente" falso (POL ≠ token ERC20).
        var senderBalance = await _leasingCoreService.GetErc20BalanceAsync(tokenAddress, senderWallet);
        if (senderBalance.HasValue && senderBalance.Value < paymentAmountBig)
        {
            var balanceFormatted = (decimal)senderBalance.Value / (decimal)Math.Pow(10, 6);
            var neededFormatted = (decimal)paymentAmountBig / (decimal)Math.Pow(10, 6);
            _logger.LogWarning("Saldo insuficiente en wallet: {Sender}. Balance: {Balance}, Necesario: {Needed}",
                senderWallet, senderBalance.Value, request.Body.PaymentAmount);
            return new CreatePaymentResponse(
                false,
                string.Empty,
                neededFormatted,
                agreement.RemainingBalance,
                $"Saldo insuficiente en la wallet que paga (token ERC20 del contrato, no POL). La wallet {senderWallet} tiene {balanceFormatted:N2} tokens. Se necesitan {neededFormatted:N2}. Recargue esa wallet con el token base del leasing antes de intentar el pago.");
        }

        if (!senderBalance.HasValue)
        {
            _logger.LogWarning(
                "No se pudo leer balanceOf del token {Token} para {Sender}; se continúa con el Paymaster y la cadena validará el saldo.",
                tokenAddress,
                senderWallet);
        }

        _logger.LogInformation("Procesando pago. Sender: {Sender}, LeasingCore: {LeasingCore}, AgreementId: {AgreementId}, Token: {Token}",
            senderWallet, agreement.LeasingCoreAddress, agreement.Id, tokenAddress);

        WebhookResponseDto webhookResponse = await _webHookService.ProcessPaymentWebhookAsync(request.Body, senderWallet, agreement.LeasingCoreAddress, tokenAddress);

        decimal newRemainingBalance = agreement.RemainingBalance;
        decimal amount = (decimal)paymentAmountBig / (decimal)Math.Pow(10, 6);

        if (webhookResponse.Status)
        {
            // Solo actualizar RemainingBalance para cuotas mensuales. El pago residual no reduce saldo (ya es 0).
            if (expectedPayment == null || !expectedPayment.IsResidualPayment)
            {
                await _agreementRepository.ProcessPaymentAsync(agreement.Id, amount);
            }
            var updatedAgreement = await _agreementRepository.GetByIdAsync(agreement.Id);
            newRemainingBalance = updatedAgreement?.RemainingBalance ?? 0;

            _logger.LogInformation("Pago procesado exitosamente. Notificando a inversores del leasing: {LeasingId}", agreement.LeasingId);

            try
            {
                var investors = await _investmentRepository.GetInvestorsByLeasingIdAsync(agreement.LeasingId);
                var investorsWithTokens = investors.Where(u => !string.IsNullOrEmpty(u.PushNotificationToken)).ToList();

                if (investorsWithTokens.Count > 0)
                {
                    var isLastPayment = expectedPayment?.IsResidualPayment ?? false;
                    var (title, body) = isLastPayment
                        ? ("Último pago realizado", $"Se completó el último pago del leasing {leasing.Name} (valor residual). El leasing ha sido finalizado.")
                        : ("Pago Recibido", $"El arrendatario ha realizado el pago mensual del canon. Ya puedes reclamar tus ganancias de {leasing.Name}.");

                    var notifications = investorsWithTokens.Select(investor => new NotificationDto
                    {
                        RecipientId = investor.PushNotificationToken!,
                        Title = title,
                        Body = body,
                        Data = new Dictionary<string, object>
                        {
                            {"type", "INVESTMENT-RETURN"},
                            {"leasingId", agreement.LeasingId.ToString()},
                            {"paymentAmount", amount.ToString()},
                            {"isLastPayment", isLastPayment}
                        }
                    });

                    await _notificationService.SendBulkNotificationsAsync(notifications);
                    _logger.LogInformation("Notificaciones enviadas a {Count} inversores", investorsWithTokens.Count);
                }
                else
                {
                    _logger.LogInformation("No se encontraron inversores con tokens de notificación push para el leasing: {LeasingId}", agreement.LeasingId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificaciones a inversores del leasing: {LeasingId}", agreement.LeasingId);
            }
        }

        PaymentLogDto paymentLog = new PaymentLogDto
        {
            UserLeasingAgreementId = agreement.Id,
            PaymentAmount = amount,
            TotalValue = agreement.TotalValue,
            RemainingBalance = newRemainingBalance,
            LeasingContractAddress = leasing.ContractAddress,
            UserWallet = user.WalletAddress ?? string.Empty,
            Hash = webhookResponse.Hash,
            Status = webhookResponse.Status
        };

        await _logRepository.InsertPaymentLogAsync(paymentLog);

        return new CreatePaymentResponse(
            webhookResponse.Status,
            webhookResponse.Hash ?? string.Empty,
            amount,
            newRemainingBalance,
            webhookResponse.ErrorMessage
        );
    }
}