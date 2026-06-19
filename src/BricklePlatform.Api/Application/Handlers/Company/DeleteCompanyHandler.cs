using BricklePlatform.Api.Application.Commands.Company;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Company;

public class DeleteCompanyHandler : IRequestHandler<DeleteCompanyCommand, Unit>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<DeleteCompanyHandler> _logger;

    public DeleteCompanyHandler(
        ICompanyRepository companyRepository,
        ILogger<DeleteCompanyHandler> logger)
    {
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);

            var company = await _companyRepository.GetByIdAsync(request.Id);
            if (company == null)
            {
                throw new ApplicationException($"Empresa con ID {request.Id} no encontrada");
            }

            await _companyRepository.DeleteAsync(request.Id);

            _logger.LogInformation("Successfully deleted company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);
            throw new ApplicationException("Se produjo un error al eliminar la empresa");
        }
    }
}