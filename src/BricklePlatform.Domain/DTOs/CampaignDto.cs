using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.DTOs;

public class CampaignDto
{
  public Guid Id { get; set; }
  public decimal MinCapital { get; set; } = 0;
  public decimal MaxCapital { get; set; } = 0;
  public CampaignStatusEnum Status { get; set; }
  public string BaseToken { get; set; }
  public string BrickleAddress { get; set; }
  public string CampaignAddress { get; set; }
  public string CampaignTx { get; set; }
  public Guid LeasingId { get; set; }

}