using BricklePlatform.Api.Application.Queries.Portfolio;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using MediatR;
using System.Numerics;

namespace BricklePlatform.Api.Application.Handlers.Portfolio
{
    public class GetPortfolioOverviewQueryHandler : IRequestHandler<GetPortfolioOverviewQuery, PortfolioOverviewDto>
    {
        private readonly IInvestmentRepository _investmentRepository;
        private readonly IUserActivityLogService _userActivityLogService;
        private readonly ILeasingCoreService _leasingCoreService;
        private readonly ILogger<GetPortfolioOverviewQueryHandler> _logger;

        public GetPortfolioOverviewQueryHandler(
            IInvestmentRepository investmentRepository,
            IUserActivityLogService userActivityLogService,
            ILeasingCoreService leasingCoreService,
            ILogger<GetPortfolioOverviewQueryHandler> logger)
        {
            _investmentRepository = investmentRepository;
            _userActivityLogService = userActivityLogService;
            _leasingCoreService = leasingCoreService;
            _logger = logger;
        }

        public async Task<PortfolioOverviewDto> Handle(GetPortfolioOverviewQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calculando overview del portfolio para usuario: {UserId}", request.UserId);

            var investments = await _investmentRepository.GetByUserIdAsync(request.UserId);
            var investmentsList = investments.ToList();

            var logWindowDays = Math.Max(730, (request.To.DayNumber - request.From.DayNumber) + 60);
            var allLogs = (await _userActivityLogService.GetUserActivityLogsAsync(
                request.UserId,
                logWindowDays,
                null,
                null,
                null)).ToList();

            // Excluir movimientos de seed internos (ya no hay endpoint dev; limpia cálculos si quedaron filas antiguas).
            allLogs = allLogs
                .Where(log => string.IsNullOrWhiteSpace(log.Reference)
                    || !log.Reference.TrimStart().StartsWith("[DEV-SEED]", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Separar logs de renta en intereses y capital devuelto.
            // Legacy INVESTMENT-RETURN (sin desglose) se trata como interés puro para backward compat.
            var interestLogs = allLogs
                .Where(log => IsInterestReturnType(log.Type))
                .ToList();

            var capitalReturnLogs = allLogs
                .Where(log => log.Type.Equals("INVESTMENT-RETURN-CAPITAL", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // returnLogs = todos los de renta (intereses + capital) — solo para cálculos de efectivo
            var returnLogs = allLogs
                .Where(log => IsAnyReturnType(log.Type))
                .ToList();

            if (!investmentsList.Any())
            {
                _logger.LogInformation("No se encontraron inversiones para el usuario: {UserId}", request.UserId);
                return CreateEmptyPortfolioOverview(request.To);
            }

            var merged = MergeInvestmentsPerLeasing(investmentsList);
            var tokenSupplyByLeasingId = await ResolveTokenSupplyByLeasingAsync(merged, cancellationToken).ConfigureAwait(false);
            var states = BuildAssetStates(merged, tokenSupplyByLeasingId);

            var months = GenerateMonthsBetween(request.From, request.To);
            var chart = CalculateMonthlyBars(investmentsList, months, interestLogs, allLogs, states);

            // totalInvested = capital inicial puesto en activos (constante)
            var totalInvested = investmentsList.Sum(i => i.Amount);

            // totalReturn = solo intereses (ganancia real sobre el capital invertido)
            var totalReturn = interestLogs
                .Where(log => log.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                .Sum(log => log.TxAmount);

            // currentValue = VALOR REAL DEL PORTAFOLIO 
            // Estrictamente: Cantidad de Bricks actuales * Precio de cada token
            var currentValue = merged.Sum(i => i.BricksCount * (i.Leasing?.PricePerToken ?? 0));

            var roi = CalculateWeightedAverageROI(investmentsList);
            _logger.LogInformation("ROI ponderado calculado: {ROI} para {InvestmentCount} inversiones con valor total invertido: {TotalInvested}",
                roi, investmentsList.Count, totalInvested);

            var projectedChart = CalculateProjectedPatrimonioChart(
                states,
                allLogs,
                monthsAhead: 12);

            _logger.LogInformation("Portfolio overview calculado exitosamente para usuario: {UserId}", request.UserId);

            return new PortfolioOverviewDto
            {
                AsOf = request.To.ToDateTime(TimeOnly.MaxValue),
                Currency = "COP",
                CurrentValue = Math.Round(currentValue, 2),
                TotalInvested = Math.Round(totalInvested, 2),
                TotalReturn = totalReturn,
                Roi = Math.Round(roi, 4),
                Chart = chart,
                ProjectedChart = projectedChart
            };
        }

        private static PortfolioOverviewDto CreateEmptyPortfolioOverview(DateOnly asOf)
        {
            return new PortfolioOverviewDto
            {
                AsOf = asOf.ToDateTime(TimeOnly.MaxValue),
                Currency = "COP",
                CurrentValue = 0m,
                TotalInvested = 0m,
                TotalReturn = 0m,
                Roi = 0m,
                Chart = new List<MonthlyBarDto>(),
                ProjectedChart = new List<ProjectionPointDto>()
            };
        }

        private static List<DateOnly> GenerateMonthsBetween(DateOnly from, DateOnly to)
        {
            var months = new List<DateOnly>();
            var current = new DateOnly(from.Year, from.Month, 1);
            var endMonth = new DateOnly(to.Year, to.Month, 1);

            while (current <= endMonth)
            {
                var lastDayOfMonth = current.AddMonths(1).AddDays(-1);
                months.Add(lastDayOfMonth);
                current = current.AddMonths(1);
            }

            return months;
        }

        private static List<MonthlyBarDto> CalculateMonthlyBars(
            List<InvestmentDto> investments,
            List<DateOnly> months,
            List<UserActivityLogDto> interestLogs,
            List<UserActivityLogDto> allLogs,
            List<OverviewAssetState> states)
        {
            var chart = new List<MonthlyBarDto>();

            foreach (var month in months)
            {
                var monthEndDt = month.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

                var investmentsUpToMonth = investments
                    .Where(i => DateOnly.FromDateTime(i.CreatedAt) <= month)
                    .ToList();

                // Return en gráfica histórica = solo intereses acumulados (ganancia real)
                var interestUpToMonth = interestLogs
                    .Where(log => DateOnly.FromDateTime(log.Timestamp) <= month
                               && log.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                    .Sum(log => log.TxAmount);

                var totalInvestedUpToMonth = investmentsUpToMonth.Sum(i => i.Amount);

                // Patrimonio = efectivo libre + valor actual de los tokens ( bricks * precio )
                var cashAtMonth = EstimateCashFromLogs(allLogs, monthEndDt);

                var asOfMonthEnd = new DateOnly(monthEndDt.Year, monthEndDt.Month, DateTime.DaysInMonth(monthEndDt.Year, monthEndDt.Month));
                
                // Calculamos el valor de los activos en esa fecha simulando la amortización 
                // para cada inversión activa hasta ese mes.
                var activosAtMonth = CalculateActivosBookAtDate(states, asOfMonthEnd);

                var patrimonio = cashAtMonth + activosAtMonth;

                chart.Add(new MonthlyBarDto
                {
                    Month = month.ToString("yyyy-MM"),
                    MonthText = GetSpanishMonthName(month.Month),
                    Value = Math.Round(patrimonio, 2),
                    Invested = Math.Round(totalInvestedUpToMonth, 2),
                    Return = Math.Round(interestUpToMonth, 2)
                });
            }

            return chart;
        }

        /// <summary>
        /// Efectivo libre reconstruido desde logs (recargas, retiros, inversiones, rentas reclamadas).
        /// Tanto el capital devuelto como los intereses recibidos se suman al efectivo disponible
        /// (ambos llegan al wallet del usuario).
        /// </summary>
        private static decimal EstimateCashFromLogs(IEnumerable<UserActivityLogDto> logs, DateTime asOfUtc)
        {
            decimal cash = 0m;
            foreach (var log in logs)
            {
                if (!log.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (log.Timestamp > asOfUtc)
                    continue;

                var t = log.Type.ToUpperInvariant();
                switch (t)
                {
                    case "RECHARGE":
                    // Rentas: legacy (interés puro) + nuevos subtipos (interés y capital devuelto)
                    // ambos suman al efectivo porque el dinero llega al wallet del usuario
                    case "INVESTMENT-RETURN":
                    case "INVESTMENT-RETURN-INTEREST":
                    case "INVESTMENT-RETURN-CAPITAL":
                        cash += log.TxAmount;
                        break;
                    case "WITHDRAW":
                    case "INVESTMENT":
                        cash -= log.TxAmount;
                        break;
                }
            }

            return cash;
        }

        /// <summary>
        /// Agrupa filas de inversión del mismo leasing para no duplicar la amortización del activo.
        /// </summary>
        private static List<InvestmentDto> MergeInvestmentsPerLeasing(IReadOnlyCollection<InvestmentDto> investments)
        {
            return investments
                .Where(i => i.Leasing?.Agreement != null)
                .GroupBy(i => i.LeasingId)
                .Select(g =>
                {
                    var rows = g.ToList();
                    var head = rows[0];
                    return new InvestmentDto
                    {
                        Id = head.Id,
                        UserId = head.UserId,
                        LeasingId = head.LeasingId,
                        Amount = rows.Sum(r => r.Amount),
                        BricksCount = rows.Sum(r => r.BricksCount),
                        BricksName = head.BricksName,
                        PaymentCount = rows.Max(r => r.PaymentCount),
                        CreatedAt = rows.Min(r => r.CreatedAt),
                        UpdatedAt = rows.Max(r => r.UpdatedAt),
                        Leasing = head.Leasing
                    };
                })
                .ToList();
        }

        private static List<OverviewAssetState> BuildAssetStates(
            List<InvestmentDto> mergedInvestments,
            Dictionary<Guid, decimal> tokenSupplyByLeasingId)
        {
            var states = new List<OverviewAssetState>();
            foreach (var i in mergedInvestments)
            {
                var agreement = i.Leasing?.Agreement;
                if (agreement == null) continue;

                var totalTokens = tokenSupplyByLeasingId.TryGetValue(i.LeasingId, out var onChainSupply) && onChainSupply > 0
                    ? onChainSupply
                    : i.Leasing!.Tokens;

                if (totalTokens <= 0 || agreement.InstallmentAmount <= 0)
                    continue;

                var userShare = (decimal)i.BricksCount / totalTokens;
                if (userShare <= 0)
                    continue;

                var monthlyRate = agreement.InstallmentRate > 0 ? agreement.InstallmentRate / 100m : 0m;
                var totalAssetValue = agreement.AssetValue > 0
                    ? agreement.AssetValue
                    : agreement.RemainingBalance;
                var managementFeeAnnualPercent = agreement.ManagementFee > 0 ? agreement.ManagementFee : 0m;

                var firstMonth = new DateOnly(i.CreatedAt.Year, i.CreatedAt.Month, 1);

                states.Add(new OverviewAssetState
                {
                    Canon = agreement.InstallmentAmount,
                    MonthlyRate = monthlyRate,
                    InitialAssetValue = totalAssetValue,
                    ManagementFeeAnnualPercent = managementFeeAnnualPercent,
                    UserShare = userShare,
                    FirstPaymentMonth = firstMonth
                });
            }
            return states;
        }

        private static decimal CalculateActivosBookAtDate(
            List<OverviewAssetState> states,
            DateOnly asOfMonthEnd)
        {
            decimal totalRemainingUser = 0m;

            foreach (var state in states)
            {
                if (asOfMonthEnd < state.FirstPaymentMonth)
                    continue;

                var currentAssetValue = state.InitialAssetValue;
                
                var cursor = state.FirstPaymentMonth;
                var cursorEnd = new DateOnly(cursor.Year, cursor.Month, DateTime.DaysInMonth(cursor.Year, cursor.Month));

                while (cursorEnd <= asOfMonthEnd)
                {
                    var grossInterest = currentAssetValue * state.MonthlyRate;
                    var localCapital = state.Canon - grossInterest;
                    currentAssetValue -= localCapital;
                    if (currentAssetValue < 0) currentAssetValue = 0;

                    cursor = cursor.AddMonths(1);
                    cursorEnd = new DateOnly(cursor.Year, cursor.Month, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                }

                totalRemainingUser += currentAssetValue * state.UserShare;
            }

            return totalRemainingUser;
        }

        private static List<ProjectionPointDto> CalculateProjectedPatrimonioChart(
            List<OverviewAssetState> originalStates,
            List<UserActivityLogDto> activityLogs,
            int monthsAhead)
        {
            var result = new List<ProjectionPointDto>();
            if (!originalStates.Any()) return result;

            var asOfUtc = DateTime.UtcNow;
            var asOfDateOnly = DateOnly.FromDateTime(asOfUtc);
            var cash = EstimateCashFromLogs(activityLogs, asOfUtc);

            if (cash < 0m && originalStates.Count > 0)
                cash = 0m;

            var currentStates = new List<CurrentAssetState>();
            foreach (var state in originalStates)
            {
                var currentAssetValue = state.InitialAssetValue;
                if (asOfDateOnly >= state.FirstPaymentMonth)
                {
                    var cursor = state.FirstPaymentMonth;
                    var cursorEnd = new DateOnly(cursor.Year, cursor.Month, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                    while (cursorEnd <= asOfDateOnly)
                    {
                        var grossInterest = currentAssetValue * state.MonthlyRate;
                        var localCapital = state.Canon - grossInterest;
                        currentAssetValue -= localCapital;
                        if (currentAssetValue < 0) currentAssetValue = 0;

                        cursor = cursor.AddMonths(1);
                        cursorEnd = new DateOnly(cursor.Year, cursor.Month, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                    }
                }

                currentStates.Add(new CurrentAssetState
                {
                    BaseState = state,
                    CurrentAssetValue = currentAssetValue
                });
            }

            var activos0 = currentStates.Sum(s => s.CurrentAssetValue * s.BaseState.UserShare);
            var patrimonio0 = cash + activos0;

            result.Add(new ProjectionPointDto
            {
                Month = GetSpanishMonthName(asOfUtc.Month),
                MonthKey = asOfDateOnly.ToString("yyyy-MM"),
                MonthIndex = 0,
                Capital = Math.Round(activos0, 2),
                Interest = 0m,
                ProjectedValue = Math.Round(patrimonio0, 2),
                CapitalReturned = 0m
            });

            var cursorProj = asOfDateOnly;

            for (int m = 1; m <= monthsAhead; m++)
            {
                cursorProj = cursorProj.AddMonths(1);
                var monthEnd = new DateOnly(
                    cursorProj.Year,
                    cursorProj.Month,
                    DateTime.DaysInMonth(cursorProj.Year, cursorProj.Month));

                decimal interestUserMonthTotal = 0m;
                decimal capitalUserMonthTotal = 0m;
                decimal remainingUserTotal = 0m;

                foreach (var stateWrapper in currentStates)
                {
                    var state = stateWrapper.BaseState;
                    if (monthEnd < state.FirstPaymentMonth || stateWrapper.CurrentAssetValue <= 0)
                    {
                        remainingUserTotal += stateWrapper.CurrentAssetValue * state.UserShare;
                        continue;
                    }

                    var grossInterest = stateWrapper.CurrentAssetValue * state.MonthlyRate;
                    var brickleInterest = stateWrapper.CurrentAssetValue * (state.ManagementFeeAnnualPercent / 100m / 12m);
                    var tokenHolderInterest = grossInterest - brickleInterest;
                    var localCapital = state.Canon - grossInterest;

                    var userInterest = tokenHolderInterest * state.UserShare;
                    var userCapital = localCapital * state.UserShare;

                    stateWrapper.CurrentAssetValue -= localCapital;
                    if (stateWrapper.CurrentAssetValue < 0) stateWrapper.CurrentAssetValue = 0;

                    interestUserMonthTotal += userInterest;
                    capitalUserMonthTotal += userCapital;
                    remainingUserTotal += stateWrapper.CurrentAssetValue * state.UserShare;
                }

                cash += interestUserMonthTotal + capitalUserMonthTotal;
                var patrimonio = cash + remainingUserTotal;

                result.Add(new ProjectionPointDto
                {
                    Month = GetSpanishMonthName(monthEnd.Month),
                    MonthKey = monthEnd.ToString("yyyy-MM"),
                    MonthIndex = m,
                    Capital = Math.Round(remainingUserTotal, 2),
                    Interest = Math.Round(interestUserMonthTotal, 2),
                    ProjectedValue = Math.Round(patrimonio, 2),
                    CapitalReturned = Math.Round(capitalUserMonthTotal, 2)
                });
            }

            return result;
        }

        private sealed class CurrentAssetState
        {
            public OverviewAssetState BaseState { get; set; } = null!;
            public decimal CurrentAssetValue { get; set; }
        }

        private async Task<Dictionary<Guid, decimal>> ResolveTokenSupplyByLeasingAsync(
            IReadOnlyCollection<InvestmentDto> investments,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<Guid, decimal>();

            var investmentByLeasing = investments
                .Where(i => i.Leasing != null)
                .GroupBy(i => i.LeasingId)
                .Select(g => g.First())
                .ToList();

            foreach (var investment in investmentByLeasing)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var leasing = investment.Leasing;
                if (leasing == null) continue;

                if (string.IsNullOrWhiteSpace(leasing.Agreement?.LeasingCoreAddress))
                {
                    if (leasing.Tokens > 0) result[investment.LeasingId] = leasing.Tokens;
                    continue;
                }

                try
                {
                    var state = await _leasingCoreService
                        .GetLeasingContractStateAsync(leasing.Agreement.LeasingCoreAddress)
                        .ConfigureAwait(false);

                    if (state?.LeasingTokenTotalSupply is BigInteger supply &&
                        TryConvertTokenSupply(supply, out var parsedSupply))
                    {
                        result[investment.LeasingId] = parsedSupply;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "No se pudo resolver LeasingTokenTotalSupply para leasing {LeasingId}. Se usa fallback DB.",
                        investment.LeasingId);
                }

                if (leasing.Tokens > 0) result[investment.LeasingId] = leasing.Tokens;
            }

            return result;
        }

        private static bool TryConvertTokenSupply(BigInteger supply, out decimal parsedSupply)
        {
            parsedSupply = 0m;
            if (supply <= 0) return false;
            if (supply > 10_000_000_000) return false;

            try
            {
                parsedSupply = (decimal)supply;
                return parsedSupply > 0;
            }
            catch
            {
                return false;
            }
        }

        private sealed class OverviewAssetState
        {
            public decimal Canon { get; set; }
            public decimal MonthlyRate { get; set; }
            public decimal InitialAssetValue { get; set; }
            public decimal ManagementFeeAnnualPercent { get; set; }
            public decimal UserShare { get; set; }
            public DateOnly FirstPaymentMonth { get; set; }
        }

        // ── Helpers de clasificación de tipos de log ──────────────────────────

        /// <summary>True si el log es una renta de tipo interés (incluye legacy INVESTMENT-RETURN).</summary>
        private static bool IsInterestReturnType(string type)
            => type.Equals("INVESTMENT-RETURN", StringComparison.OrdinalIgnoreCase)
            || type.Equals("INVESTMENT-RETURN-INTEREST", StringComparison.OrdinalIgnoreCase);

        /// <summary>True si el log es cualquier tipo de retorno de renta (interés o capital).</summary>
        private static bool IsAnyReturnType(string type)
            => IsInterestReturnType(type)
            || type.Equals("INVESTMENT-RETURN-CAPITAL", StringComparison.OrdinalIgnoreCase);

        private static string GetSpanishMonthName(int month)
        {
            return month switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => "Desconocido"
            };
        }

        private static decimal CalculateWeightedAverageROI(List<InvestmentDto> investments)
        {
            if (!investments.Any())
                return 0m;

            var validInvestments = investments
                .Where(i => i.Leasing != null && i.Leasing.TIR > 0 && i.BricksCount > 0)
                .ToList();

            if (!validInvestments.Any())
            {
                var totalAmount = investments.Sum(i => i.Amount);
                if (totalAmount <= 0) return 0m;
                return investments
                    .Where(i => i.Leasing != null)
                    .Sum(i => i.Amount * i.Leasing!.TIR) / totalAmount;
            }

            var totalWeight = validInvestments.Sum(i => (decimal)i.BricksCount * i.Leasing!.TIR);
            if (totalWeight <= 0) return 0m;

            return validInvestments.Sum(i => ((decimal)i.BricksCount * i.Leasing!.TIR / totalWeight) * i.Leasing!.TIR);
        }
    }
}
