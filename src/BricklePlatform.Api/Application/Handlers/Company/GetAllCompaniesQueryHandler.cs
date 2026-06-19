using BricklePlatform.Api.Application.Queries.Company;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Company;

public class GetAllCompaniesQueryHandler : IRequestHandler<GetAllCompaniesQuery, IEnumerable<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<GetAllCompaniesQueryHandler> _logger;

    public GetAllCompaniesQueryHandler(
        ICompanyRepository companyRepository,
        ILogger<GetAllCompaniesQueryHandler> logger)
    {
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CompanyDto>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting all companies. CorrelationId: {CorrelationId}",
                request.Header.CorrelationId);

            var companies = await _companyRepository.GetAllAsync();

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
            _logger.LogError(ex, "Error getting all companies. CorrelationId: {CorrelationId}",
                request.Header.CorrelationId);
            throw new ApplicationException("Se produjo un error al obtener las empresas");
        }
    }
}