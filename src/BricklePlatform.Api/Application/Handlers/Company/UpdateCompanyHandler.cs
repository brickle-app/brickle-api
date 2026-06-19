using BricklePlatform.Api.Application.Commands.Company;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Company;

public class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<UpdateCompanyHandler> _logger;

    public UpdateCompanyHandler(
        ICompanyRepository companyRepository,
        ILogger<UpdateCompanyHandler> logger)
    {
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);

            var company = await _companyRepository.GetByIdAsync(request.Id);
            if (company == null)
            {
                throw new ApplicationException($"Empresa con ID {request.Id} no encontrada");
            }

            company.Update(
                name: request.Body.Name,
                operationTime: request.Body.OperationTime,
                operationMeasure: request.Body.OperationMeasure,
                creditRating: request.Body.CreditRating,
                leasingContract: request.Body.LeasingContract
            );

            await _companyRepository.UpdateAsync(company);

            _logger.LogInformation("Successfully updated company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                company.Id, request.Header.CorrelationId);

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
            _logger.LogError(ex, "Error updating company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);
            throw new ApplicationException("Se produjo un error al actualizar la empresa");
        }
    }
}