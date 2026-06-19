using BricklePlatform.Api.Application.Queries.Portfolio;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;
using System.Numerics;

namespace BricklePlatform.Api.Application.Handlers.Portfolio
{
    public class GetPortfolioProjectionsHandler : IRequestHandler<GetPortfolioProjectionsQuery, List<ProjectionPointDto>>
    {
        private readonly ILogger<GetPortfolioProjectionsHandler> _logger;
        private readonly IInvestmentRepository _investmentRepository;
        private readonly ILeasingCoreService _leasingCoreService;

        public GetPortfolioProjectionsHandler(
            ILogger<GetPortfolioProjectionsHandler> logger,
            IInvestmentRepository investmentRepository,
            ILeasingCoreService leasingCoreService)
        {
            _logger = logger;
            _investmentRepository = investmentRepository;
            _leasingCoreService = leasingCoreService;
        }

        public async Task<List<ProjectionPointDto>> Handle(GetPortfolioProjectionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calculando proyecciones del portfolio para usuario: {UserId}, Valor actual: {CurrentValue}, Meses: {ProjectionMonths}", 
                request.UserId, request.CurrentValue, request.ProjectionMonths);

            if (request.ProjectionMonths <= 0)
            {
                _logger.LogWarning("Número de meses de proyección inválido: {ProjectionMonths}", request.ProjectionMonths);
                return new List<ProjectionPointDto>();
            }

            if (request.CurrentValue <= 0)
            {
                _logger.LogWarning("Valor actual del portfolio inválido: {CurrentValue}", request.CurrentValue);
                return new List<ProjectionPointDto>();
            }

            if (request.ExpectedAnnualReturn < -1 || request.ExpectedAnnualReturn > 10) // Límites razonables -100% a 1000%
            {
                _logger.LogWarning("Retorno anual esperado fuera de límites razonables: {ExpectedAnnualReturn}", request.ExpectedAnnualReturn);
                return new List<ProjectionPointDto>();
            }

            var investments = (await _investmentRepository.GetByUserIdAsync(request.UserId)).ToList();
            if (!investments.Any())
            {
                _logger.LogInformation("Sin inversiones para usuario {UserId}; se retorna proyección vacía", request.UserId);
                return new List<ProjectionPointDto>();
            }

            var tokenSupplyByLeasingId = await ResolveTokenSupplyByLeasingAsync(investments, cancellationToken);

            var projection = CalculateProjectionFromCanonAndTokenShare(
                investments,
                request.ProjectionMonths,
                request.StartDate,
                tokenSupplyByLeasingId);

            _logger.LogInformation("Proyecciones calculadas exitosamente para usuario: {UserId}. {Count} puntos de proyección generados", 
                request.UserId, projection.Count);

            return projection;
        }

        /// <summary>
        /// Se agrega mes a mes para todos los activos en los que invirtió el usuario.
        /// El campo ProjectedValue representa principal pendiente del usuario al cierre.
        /// </summary>
        private static List<ProjectionPointDto> CalculateProjectionFromCanonAndTokenShare(
            IReadOnlyCollection<InvestmentDto> investments,
            int projectionMonths,
            DateOnly startDate,
            IReadOnlyDictionary<Guid, decimal> tokenSupplyByLeasingId)
        {
            var projection = new List<ProjectionPointDto>();
            var currentMonthCursor = startDate;

            var states = investments
                .Select(i =>
                {
                    if (i.Leasing?.Agreement == null || i.BricksCount <= 0)
                        return null;

                    var totalTokens = tokenSupplyByLeasingId.TryGetValue(i.LeasingId, out var onChainSupply) && onChainSupply > 0
                        ? onChainSupply
                        : i.Leasing.Tokens;

                    if (totalTokens <= 0 || i.Leasing.Agreement.InstallmentAmount <= 0)
                    {
                        return null;
                    }

                    var agreement = i.Leasing!.Agreement!;
                    var userShare = (decimal)i.BricksCount / totalTokens;
                    var monthlyRate = agreement.InstallmentRate > 0 ? agreement.InstallmentRate / 100m : 0m;
                    var totalAssetValue = agreement.AssetValue > 0
                        ? agreement.AssetValue
                        : agreement.RemainingBalance;
                    var managementFeeAnnualPercent = agreement.ManagementFee > 0 ? agreement.ManagementFee : 0m;

                    return new AssetProjectionState
                    {
                        Canon = agreement.InstallmentAmount,
                        MonthlyRate = monthlyRate,
                        LocalAssetValue = totalAssetValue,
                        ManagementFeeAnnualPercent = managementFeeAnnualPercent,
                        UserShare = userShare
                    };
                })
                .Where(s => s != null)
                .Select(s => s!)
                .ToList();

            if (!states.Any())
                return projection;

            for (int m = 1; m <= projectionMonths; m++)
            {
                currentMonthCursor = currentMonthCursor.AddMonths(1);
                var currentMonth = new DateOnly(
                    currentMonthCursor.Year,
                    currentMonthCursor.Month,
                    DateTime.DaysInMonth(currentMonthCursor.Year, currentMonthCursor.Month));

                decimal interestUserMonthTotal = 0m;
                decimal capitalUserMonthTotal = 0m;
                decimal remainingUserTotal = 0m;

                foreach (var state in states)
                {
                    if (state.LocalAssetValue <= 0)
                    {
                        remainingUserTotal += 0m;
                        continue;
                    }

                    var grossInterest = state.LocalAssetValue * state.MonthlyRate;

                    var brickleInterest = state.LocalAssetValue * (state.ManagementFeeAnnualPercent / 100m / 12m);
                    var tokenHolderInterest = grossInterest - brickleInterest;
                    var localCapital = state.Canon - grossInterest;

                    var userInterest = tokenHolderInterest * state.UserShare;
                    var userCapital = localCapital * state.UserShare;

                    state.LocalAssetValue -= localCapital;

                    interestUserMonthTotal += userInterest;
                    capitalUserMonthTotal += userCapital;
                    remainingUserTotal += state.LocalAssetValue * state.UserShare;
                }

                projection.Add(new ProjectionPointDto
                {
                    Month = GetSpanishMonthName(currentMonth.Month),
                    MonthKey = currentMonth.ToString("yyyy-MM"),
                    MonthIndex = m,
                    Capital = Math.Round(capitalUserMonthTotal, 2),
                    Interest = Math.Round(interestUserMonthTotal, 2),
                    ProjectedValue = Math.Round(remainingUserTotal, 2),
                    CapitalReturned = Math.Round(capitalUserMonthTotal, 2)
                });
            }

            return projection;
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
                var leasing = investment.Leasing;
                if (leasing == null) continue;

                // Fallback inmediato a DB si no hay dirección de core válida.
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

            // Guard-rail para evitar interpretar supplies con decimales implícitos (ej: 1e18) como "tokens emitidos".
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

        private sealed class AssetProjectionState
        {
            public decimal Canon { get; set; }
            public decimal MonthlyRate { get; set; }
            public decimal LocalAssetValue { get; set; }
            public decimal ManagementFeeAnnualPercent { get; set; }
            public decimal UserShare { get; set; }
        }

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
    }
}