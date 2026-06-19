using System;

namespace BricklePlatform.Api.Application.Dtos;

public class UpdateUserLeasingAgreementDto
{
    public decimal RemainingBalance { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
} 