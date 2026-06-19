using BricklePlatform.Api.Application.Queries.Investment;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Investment
{
    public class GetAllInvestmentsQueryHandler : IRequestHandler<GetAllInvestmentsQuery, IEnumerable<InvestmentDto>>
    {
        private readonly IInvestmentRepository _investmentRepository;
        private readonly ILogger<GetAllInvestmentsQueryHandler> _logger;

        public GetAllInvestmentsQueryHandler(
            IInvestmentRepository investmentRepository,
            ILogger<GetAllInvestmentsQueryHandler> logger)
        {
            _investmentRepository = investmentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<InvestmentDto>> Handle(GetAllInvestmentsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo todas las inversiones");
            
            var investments = await _investmentRepository.GetAllAsync();
            
            _logger.LogInformation("Se obtuvieron {Count} inversiones exitosamente", investments.Count());
            
            return investments;
        }
    }
}