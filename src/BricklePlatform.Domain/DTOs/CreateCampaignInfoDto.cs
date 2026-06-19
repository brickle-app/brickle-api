namespace BricklePlatform.Domain.DTOs;

public class CreateCampaignInfoDto
{
  public CampaignInfoDto campaignInfo { get; set; }
  public LeasingInfoDto _leasingInfo { get; set; }
}