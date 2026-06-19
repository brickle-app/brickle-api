using System.ComponentModel.DataAnnotations.Schema;
using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.Entities;

public class Campaign
{
  public Guid Id { get; private set; }
  public Guid LeasingId { get; private set; }
  public decimal MinCapital { get; private set; }
  public decimal MaxCapital { get; private set; }
  public CampaignStatusEnum Status { get; private set; }
  public string BaseToken { get; private set; }
  public string BrickleAddress { get; private set; }
  public string CampaignAddress { get; private set; }
  public string CampaignTx { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  [ForeignKey("LeasingId")]
  public virtual Leasing Leasing { get; private set; }

  private Campaign() { }

  public static Campaign Create(
    Guid LeasingId,
    decimal minCapital,
    decimal maxCapital,
    CampaignStatusEnum status,
    string baseToken,
    string brickleAddress,
    string campaignAddress,
    string campaignTx)
  {
    return new Campaign
    {
      Id = Guid.NewGuid(),
      LeasingId = LeasingId,
      MinCapital = minCapital,
      MaxCapital = maxCapital,
      Status = status,
      BaseToken = baseToken,
      BrickleAddress = brickleAddress,
      CampaignAddress = campaignAddress,
      CampaignTx = campaignTx,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };
  }

  public void Update(
    decimal minCapital,
    decimal maxCapital,
    CampaignStatusEnum status)
  {
    MinCapital = minCapital;
    MaxCapital = maxCapital;
    Status = status;
    UpdatedAt = DateTime.UtcNow;
  }
}