using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Exceptions;
using BricklePlatform.Infrastructure.Interfaces;
using BricklePlatform.Infrastructure.Models;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using BricklePlatform.Infrastructure.Constants;
using BricklePlatform.Domain.Models;
using System.Numerics;

namespace BricklePlatform.Infrastructure.Services;

public class WebHookService : IWebHookService
{
    private const string PAYMENT_COMMAND = "payment";
    private const string COMMIT_FUNDS_COMMAND = "commit";
    private const string CLAIM_RENT_COMMAND = "claimrent";

    private readonly ILogger<WebHookService> _logger;
    private readonly IHttpClientService _httpClientService;
    private readonly IOptions<InfrastructureSettings> _settings;


    public WebHookService(
        ILogger<WebHookService> logger,
        IHttpClientService httpClientService,
        IOptions<InfrastructureSettings> settings)
    {
        _logger = logger;
        _httpClientService = httpClientService;
        _settings = settings;
    }

    public async Task<WebhookResponseDto> ProcessPaymentWebhookAsync(PaymentDto paymentDto, string walletAddress, string leasingContractAddress, string tokenAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
                throw new InfrastructureException("La wallet de pago (Brickle) no está configurada. Configure Web3Settings:PaymentWalletAddress.");
            if (string.IsNullOrWhiteSpace(leasingContractAddress) || leasingContractAddress == "0x0000000000000000000000000000000000000000")
                throw new InfrastructureException("La dirección del LeasingCore no está configurada para este acuerdo. Verifique que la campaña se haya finalizado correctamente.");

            _logger.LogInformation("Iniciando procesamiento de pago para el contrato de arrendamiento de usuario: {UserLeasingAgreementId}",
                leasingContractAddress);

            var permitSignature = new
            {
                v = paymentDto.PermitSignature.V,
                r = paymentDto.PermitSignature.R,
                s = paymentDto.PermitSignature.S
            };


            var requestBody = new
            {
                @params = new[]
                {
                    new
                    {
                        command = PAYMENT_COMMAND,
                        token = tokenAddress,
                        sender = walletAddress,
                        leasingCore = leasingContractAddress,
                        amount = paymentDto.PaymentAmount,
                        fee = "100000",
                        deadline = paymentDto.Deadline.ToString(),
                        permitSignature
                    }
                }
            };

            _logger.LogDebug("Enviando pago al webhook: {RequestBody}", JsonConvert.SerializeObject(requestBody));

            RequestHttpModel request = new()
            {
                HttpClientName = "Webhook",
                Method = "POST",
                Url = $"{_settings.Value.WebhookSettings.Url}",
                Body = JsonConvert.SerializeObject(requestBody)
            };

            (bool success, string response) = await _httpClientService.MakeRequestWithHeaders(request);

            if (!success)
            {
                throw new InfrastructureException($"Error al procesar el pago: {response}");
            }

            var webhookResponse = WebhookResponseDto.FromWebhookResult(response);

            if (!webhookResponse.Status)
            {
                _logger.LogWarning("Webhook devolvió status false. Sender: {Sender}, LeasingCore: {LeasingCore}, Hash: {Hash}, Error: {Error}. Respuesta completa: {Response}",
                    walletAddress, leasingContractAddress, webhookResponse.Hash, webhookResponse.ErrorMessage, response);
            }
            else
            {
                _logger.LogInformation("Pago procesado exitosamente para el contrato de arrendamiento de usuario: {UserLeasingAgreementId}, Hash: {Hash}",
                    leasingContractAddress, webhookResponse.Hash);
            }

            return webhookResponse;
        }
        catch (InfrastructureException ex)
        {
            // Cuando el webhook devuelve 500, el HttpClient incluye el body en ex.Message.
            // Extraer y parsear para devolver error legible al frontend en vez de relanzar.
            string? bodyJson = TryExtractJsonFromHttpError(ex.Message);
            if (!string.IsNullOrEmpty(bodyJson))
            {
                try
                {
                    var errorResponse = WebhookResponseDto.FromWebhookResult(bodyJson);
                    _logger.LogError(ex, "Error en webhook de pago. Sender: {Sender}, LeasingCore: {LeasingCore}, Error: {Error}",
                        walletAddress, leasingContractAddress, errorResponse.ErrorMessage);
                    return errorResponse;
                }
                catch
                {
                    /* Fallback a relanzar */
                }
            }

            _logger.LogError(ex, "Error al procesar pago: {Message}", ex.Message);
            throw new InfrastructureException($"Error al procesar pago: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            string errorMessage = $"Error al deserializar la respuesta del webhook: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new InfrastructureException(errorMessage, ex);
        }
    }

    /// <summary>
    /// Extrae el JSON del body cuando el HttpClient lanza con "Error en la respuesta HTTP: 500 - {json}".
    /// También maneja formato con " - " o JSON que empieza con "{".
    /// </summary>
    private static string? TryExtractJsonFromHttpError(string message)
    {
        if (string.IsNullOrEmpty(message)) return null;

        int startIdx = message.IndexOf('{', StringComparison.Ordinal);
        if (startIdx < 0) return null;

        string candidate = message[startIdx..].Trim();
        return candidate.Length > 1 ? candidate : null;
    }

    public async Task<WebhookResponseDto> ProcessCommitFunds(CommitFundsDto commitFundsDto, int deadline, PermitSignatureDto permit)
    {
        try
        {
            _logger.LogInformation("Iniciando procesamiento de compromiso de fondos de usuario: {UserWallet}",
                commitFundsDto.Sender);

            var permitSignature = new
            {
                v = permit.V,
                r = permit.R,
                s = permit.S
            };

            var requestBody = new
            {
                @params = new[]
                {
                    new
                    {
                        command = COMMIT_FUNDS_COMMAND,
                        token = commitFundsDto.Token,
                        sender = commitFundsDto.Sender,
                        campaign = commitFundsDto.Campaign,
                        amount = commitFundsDto.Amount,
                        fee = commitFundsDto.Fee,
                        deadline,
                        permitSignature
                    }
                }
            };

            RequestHttpModel request = new()
            {
                HttpClientName = "Webhook",
                Method = "POST",
                Url = $"{_settings.Value.WebhookSettings.Url}",
                Body = JsonConvert.SerializeObject(requestBody)
            };

            (bool success, string response) = await _httpClientService.MakeRequestWithHeaders(request);

            if (!success)
            {
                throw new InfrastructureException($"Error al procesar el compromiso de fondos: {response}");
            }

            var webhookResponse = WebhookResponseDto.FromWebhookResult(response);

            _logger.LogInformation("Pago procesado exitosamente para la campaña : {Camapign}",
                commitFundsDto.Campaign);

            return webhookResponse;
        }
        catch (InfrastructureException ex)
        {
            string errorMessage = $"Error al procesar compromiso de fondos: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new InfrastructureException(errorMessage, ex);
        }
        catch (JsonException ex)
        {
            string errorMessage = $"Error al deserializar la respuesta del webhook: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new InfrastructureException(errorMessage, ex);
        }
    }

    public async Task<WebhookResponseDto> ProcessClaimRent(string token, string leasingCore, string receiver, int fee, int deadline, PermitSignatureDto permitSignature)
    {
        try
        {
            _logger.LogInformation("Iniciando procesamiento de reclamo de renta para contrato: {LeasingCore}",
                leasingCore);

            var permitSignatureObj = new
            {
                v = permitSignature.V,
                r = permitSignature.R,
                s = permitSignature.S
            };

            var requestBody = new
            {
                @params = new[]
                {
                    new
                    {
                        command = CLAIM_RENT_COMMAND,
                        token,
                        leasingCore,
                        receiver,
                        fee,
                        deadline,
                        permitSignature = permitSignatureObj
                    }
                }
            };

            RequestHttpModel request = new()
            {
                HttpClientName = "Webhook",
                Method = "POST",
                Url = $"{_settings.Value.WebhookSettings.Url}",
                Body = JsonConvert.SerializeObject(requestBody)
            };

            (bool success, string response) = await _httpClientService.MakeRequestWithHeaders(request);

            if (!success)
            {
                throw new InfrastructureException($"Error al procesar el reclamo de renta: {response}");
            }

            var webhookResponse = WebhookResponseDto.FromWebhookResult(response);

            _logger.LogInformation("Reclamo de renta procesado exitosamente para el contrato: {LeasingCore}",
                leasingCore);

            return webhookResponse;
        }
        catch (InfrastructureException ex)
        {
            string errorMessage = $"Error al procesar reclamo de renta: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new InfrastructureException(errorMessage, ex);
        }
        catch (JsonException ex)
        {
            string errorMessage = $"Error al deserializar la respuesta del webhook: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new InfrastructureException(errorMessage, ex);
        }
    }
}