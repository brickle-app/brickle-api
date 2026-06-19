using BricklePlatform.Domain.Models;

namespace BricklePlatform.Domain.DTOs;

public class ClaimRentDto
{
    public string Token { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0;
    public int Deadline { get; set; }
    public PermitSignatureDto PermitSignature { get; set; } = new();
}