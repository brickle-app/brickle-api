using BricklePlatform.Api.Application.Queries.Investment;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Investment
{
    public class GetInvestmentsByUserIdQueryHandler : IRequestHandler<GetInvestmentsByUserIdQuery, IEnumerable<InvestmentDto>>
    {
        private readonly IInvestmentRepository _investmentRepository;
        private readonly ILogger<GetInvestmentsByUserIdQueryHandler> _logger;

        public GetInvestmentsByUserIdQueryHandler(
            IInvestmentRepository investmentRepository,
            ILogger<GetInvestmentsByUserIdQueryHandler> logger)
        {
            _investmentRepository = investmentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<InvestmentDto>> Handle(GetInvestmentsByUserIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo inversiones para el usuario: {UserId}", request.UserId);

            var investments = (await _investmentRepository.GetByUserIdAsync(request.UserId)).ToList();

            _logger.LogInformation("Se obtuvieron {Count} inversiones para el usuario: {UserId}",
                investments.Count, request.UserId);

            return investments;
        }
    }
}
