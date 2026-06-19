namespace BricklePlatform.Infrastructure.Models;

public class CampaignCreatedEvent
{
  public string Campaign { get; set; }
  public string MinCap { get; set; }
  public string MaxCap { get; set; }
  public string Deadline { get; set; }
}