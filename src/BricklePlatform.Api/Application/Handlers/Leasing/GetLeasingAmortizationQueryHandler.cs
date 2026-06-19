using BricklePlatform.Api.Application.Queries.Leasing;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BricklePlatform.Api.Application.Handlers.Leasing;

public class GetLeasingAmortizationQueryHandler : IRequestHandler<GetLeasingAmortizationQuery, AmortizationTableDto>
{
    private readonly ILeasingRepository _leasingRepository;
    private readonly IUserLeasingAgreementRepository _agreementRepository;

    public GetLeasingAmortizationQueryHandler(ILeasingRepository leasingRepository, IUserLeasingAgreementRepository agreementRepository)
    {
        _leasingRepository = leasingRepository;
        _agreementRepository = agreementRepository;
    }

    public async Task<AmortizationTableDto> Handle(GetLeasingAmortizationQuery request, CancellationToken cancellationToken)
    {
        var leasing = await _leasingRepository.GetByIdAsync(request.LeasingId);
        if (leasing == null)
            throw new KeyNotFoundException($"Leasing with ID {request.LeasingId} not found");

        var agreement = await _agreementRepository.GetByLeasingIdAsync(leasing.Id);

        // Replicating logic from bloom-mobile-app/src/utils/calculateAmortization.ts

        // Months determination
        int contractMonths = 0;
        if (agreement != null)
        {
            contractMonths = (int)agreement.TermTime;
        }
        else if (leasing.ContractTime.HasValue)
        {
            // Fallback: If it's stored as a date, calculate months from creation to contract time
            // If Year < 100, it might be stored as an offset (legacy workaround)
            if (leasing.ContractTime.Value.Year < 100)
            {
                contractMonths = leasing.ContractTime.Value.Year;
            }
            else
            {
                var diff = ((leasing.ContractTime.Value.Year - leasing.CreatedAt.Year) * 12) +
                            leasing.ContractTime.Value.Month - leasing.CreatedAt.Month;
                contractMonths = Math.Max(0, diff);
            }
        }

        decimal totalValue = agreement?.TotalValue ?? leasing.Price;
        decimal installmentAmount = agreement?.InstallmentAmount ?? 0;
        decimal annualRate = leasing.TIR / 100m;
        decimal monthlyRate = annualRate / 12m;

        var table = new AmortizationTableDto
        {
            LeasingId = leasing.Id,
            TotalInstallment = installmentAmount
        };

        decimal remainingBalance = totalValue;

        for (int month = 1; month <= contractMonths; month++)
        {
            // Calculate interest for this period
            decimal interest = remainingBalance * monthlyRate;

            // Calculate capital return (principal payment)
            decimal capitalReturn = installmentAmount - interest;

            // Update remaining balance
            remainingBalance = Math.Max(0, remainingBalance - capitalReturn);

            table.Periods.Add(new AmortizationPeriodDto
            {
                Month = month,
                MonthLabel = $"M{month}",
                Installment = installmentAmount,
                CapitalReturn = Math.Max(0, capitalReturn),
                Interest = Math.Max(0, interest),
                RemainingBalance = Math.Round(remainingBalance, 2)
            });
        }

        return table;
    }
}
