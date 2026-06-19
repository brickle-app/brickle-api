namespace BricklePlatform.Domain.DTOs;

public class CommitFundsDto
{
  public string Token { get; set; }
  public string Sender { get; set; }
  public string Campaign { get; set; }
  public string Amount { get; set; }
  public string Fee { get; set; }
}