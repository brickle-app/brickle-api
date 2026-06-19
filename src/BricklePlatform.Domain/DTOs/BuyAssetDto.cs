using BricklePlatform.Domain.Models;

namespace BricklePlatform.Domain.DTOs;

public class BuyAssetDto
{
  public string Token { get; set; }
  public string Sender { get; set; }
  public decimal Amount { get; set; }
  public int Deadline { get; set; }
  public decimal TotalTokens { get; set; }
  public PermitSignatureDto PermitSignature { get; set; }
}