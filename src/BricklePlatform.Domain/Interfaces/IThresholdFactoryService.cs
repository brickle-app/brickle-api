using BricklePlatform.Domain.DTOs;

namespace BricklePlatform.Domain.Interfaces;

public interface IThresholdFactoryService
{
  Task<(string ContractAddress, string TransactionHash)> CreateCampaign(CampaignInfoDto campaignInfo, LeasingInfoDto leasingInfo);
  /// <summary>
  /// Gets the campaign state from the blockchain. Returns 0=Active, 1=Successful, 2=Failed.
  /// </summary>
  Task<byte> GetCampaignStateAsync(string campaignAddress);
  /// <summary>
  /// Finalizes a campaign on-chain and returns the leasingCore and token addresses for persistence.
  /// </summary>
  /// <param name="campaignAddress">Address of the Campaign contract.</param>
  /// <param name="brickleAssumeInsurance">Whether Brickle assumes insurance (passed to the contract).</param>
  /// <returns>LeasingCore address (for Leasing.contract_address and UserLeasingAgreement.leasing_address), token address, and transaction hash.</returns>
  Task<(string LeasingCoreAddress, string TokenAddress, string TransactionHash)> FinalizeCampaign(string campaignAddress, bool brickleAssumeInsurance = false);
}