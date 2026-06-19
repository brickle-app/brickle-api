using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Models;

namespace BricklePlatform.Domain.Interfaces;

public interface IWebHookService
{
    Task<WebhookResponseDto> ProcessPaymentWebhookAsync(PaymentDto paymentDto, string walletAddress, string leasingContractAddress, string tokenAddress);
    Task<WebhookResponseDto> ProcessCommitFunds(CommitFundsDto commitFundsDto, int deadline, PermitSignatureDto permitSignature);
    Task<WebhookResponseDto> ProcessClaimRent(string token, string leasingCore, string receiver, int fee, int deadline, PermitSignatureDto permitSignature);
}