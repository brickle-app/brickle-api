namespace BricklePlatform.Domain.DTOs
{
    public class PortfolioOverviewDto
    {
        public DateTime AsOf { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal CurrentValue { get; set; }
        /// <summary>Suma del capital invertido en activos (sin incluir retornos reclamados en el cómputo del valor de cartera).</summary>
        public decimal TotalInvested { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal Roi { get; set; }
        public IReadOnlyList<MonthlyBarDto> Chart { get; set; } = new List<MonthlyBarDto>();
        /// <summary>
        /// Proyección futura: patrimonio (efectivo estimado + valor en libros de activos) mes a mes,
        /// con amortización alineada al canon del acuerdo y efectivo desde logs (recargas, retiros, inversiones, rentas).
        /// </summary>
        public IReadOnlyList<ProjectionPointDto> ProjectedChart { get; set; } = new List<ProjectionPointDto>();
    }

    public class MonthlyBarDto
    {
        public string Month { get; set; } = string.Empty;
        public string MonthText { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Invested { get; set; }
        public decimal Return { get; set; }
    }

    public class ProjectionPointDto
    {
        public string Month { get; set; } = string.Empty;
        /// <summary>Cierre del mes en formato yyyy-MM (eje X sin ambigüedad al cruzar años).</summary>
        public string MonthKey { get; set; } = string.Empty;
        /// <summary>Número de mes desde inicio (1-based).</summary>
        public int MonthIndex { get; set; }
        /// <summary>Capital total invertido (constante en el tiempo).</summary>
        public decimal Capital { get; set; }
        /// <summary>Intereses acumulados hasta este mes (suma de todos los activos).</summary>
        public decimal Interest { get; set; }
        /// <summary>Patrimonio proyectado al cierre del mes (efectivo + participación en saldo vivo del activo).</summary>
        public decimal ProjectedValue { get; set; }
        /// <summary>Capital regresado al usuario en ese mes (proyección de flujo, uso legado).</summary>
        public decimal CapitalReturned { get; set; }
    }
}