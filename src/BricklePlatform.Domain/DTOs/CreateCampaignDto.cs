using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.DTOs;

public class CreateCampaignDto
{
  public Guid LeasingId { get; set; }
  public decimal MinCapital { get; set; } = 0;
  public decimal MaxCapital { get; set; } = 0;
  public CampaignStatusEnum Status { get; set; } = CampaignStatusEnum.Active;
  public string BaseToken { get; set; }
  public string BrickleAddress { get; set; }
}