using BricklePlatform.Api.Application.Queries.Company;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Company;

public class GetCompaniesByUserIdQueryHandler : IRequestHandler<GetCompaniesByUserIdQuery, IEnumerable<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<GetCompaniesByUserIdQueryHandler> _logger;

    public GetCompaniesByUserIdQueryHandler(
        ICompanyRepository companyRepository,
        ILogger<GetCompaniesByUserIdQueryHandler> logger)
    {
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CompanyDto>> Handle(GetCompaniesByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting companies for user ID: {UserId}. CorrelationId: {CorrelationId}",
                request.UserId, request.Header.CorrelationId);

            var companies = await _companyRepository.GetAllByUserIdAsync(request.UserId);

            return companies.Select(company => new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                OperationTime = company.OperationTime,
                OperationMeasure = company.OperationMeasure,
                CreditRating = company.CreditRating,
                LeasingContract = company.LeasingContract,
                UserId = company.UserId,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting companies for user ID: {UserId}. CorrelationId: {CorrelationId}",
                request.UserId, request.Header.CorrelationId);
            throw new ApplicationException("Se produjo un error al obtener las empresas del usuario");
        }
    }
}
