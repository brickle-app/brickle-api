namespace BricklePlatform.Api.Application.Commands.Campaign;

/// <summary>
/// Response after finalizing a campaign: success flag and addresses returned by the smart contract.
/// </summary>
public record FinalizeCampaignResponse(
    bool Success,
    string LeasingCoreAddress,
    string TokenAddress,
    string? TransactionHash
);
