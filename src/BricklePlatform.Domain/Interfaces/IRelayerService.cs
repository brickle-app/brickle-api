using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.DTOs.Relayer;
using BricklePlatform.Domain.Models;

namespace BricklePlatform.Domain.Interfaces;

public interface IRelayerService
{
    Task<RelayerTransactionResponseDto> SponsorCommitAsync(CommitFundsDto commitFundsDto, int deadline, PermitSignatureDto permitSignature, CancellationToken cancellationToken = default);
    Task<RelayerTransactionResponseDto> SponsorPaymentAsync(PaymentDto paymentDto, string walletAddress, string leasingContractAddress, string tokenAddress, CancellationToken cancellationToken = default);
    Task<RelayerTransactionResponseDto> SponsorClaimRentAsync(string token, string leasingCore, string receiver, int fee, int deadline, PermitSignatureDto permitSignature, CancellationToken cancellationToken = default);
    Task<RelayerTransactionResponseDto> SponsorAsync(RelayerSponsorRequestDto request, CancellationToken cancellationToken = default);
    Task<RelayerStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);
}
