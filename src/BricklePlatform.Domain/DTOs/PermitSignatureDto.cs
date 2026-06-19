using System.Numerics;

namespace BricklePlatform.Domain.Models;

public class PermitSignatureDto
{
  public short V { get; set; }
  public string R { get; set; }
  public string S { get; set; }
}