using BricklePlatform.Api.Application.Queries.Company;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Company;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<GetCompanyByIdQueryHandler> _logger;

    public GetCompanyByIdQueryHandler(
        ICompanyRepository companyRepository,
        ILogger<GetCompanyByIdQueryHandler> logger)
    {
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);

            var company = await _companyRepository.GetByIdAsync(request.Id);
            
            if (company == null)
            {
                _logger.LogWarning("Company with ID: {CompanyId} not found. CorrelationId: {CorrelationId}",
                    request.Id, request.Header.CorrelationId);
                return null;
            }

            return new CompanyDto
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
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);
            throw new ApplicationException("Se produjo un error al obtener la empresa");
        }
    }
}