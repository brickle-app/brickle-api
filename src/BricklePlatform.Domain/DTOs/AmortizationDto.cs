using System;
using System.Collections.Generic;

namespace BricklePlatform.Domain.DTOs;

public class AmortizationPeriodDto
{
    public int Month { get; set; }
    public string MonthLabel { get; set; } = null!;
    public decimal Installment { get; set; }
    public decimal CapitalReturn { get; set; }
    public decimal Interest { get; set; }
    public decimal RemainingBalance { get; set; }
}

public class AmortizationTableDto
{
    public Guid LeasingId { get; set; }
    public decimal TotalInstallment { get; set; }
    public List<AmortizationPeriodDto> Periods { get; set; } = new();
}
