using BricklePlatform.Api.Application.Commands.Company;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Company;

public class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CreateCompanyHandler> _logger;

    public CreateCompanyHandler(
        ICompanyRepository companyRepository,
        IUserRepository userRepository,
        ILogger<CreateCompanyHandler> logger)
    {
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new company for user: {UserId}. CorrelationId: {CorrelationId}",
                request.Body.UserId, request.Header.CorrelationId);

            // Check if user exists
            var user = await _userRepository.GetByIdAsync(request.Body.UserId);
            if (user == null)
            {
                throw new ApplicationException($"Usuario con ID {request.Body.UserId} no encontrado");
            }



            var company = Domain.Entities.Company.Create(
                name: request.Body.Name,
                operationTime: request.Body.OperationTime,
                operationMeasure: request.Body.OperationMeasure,
                creditRating: request.Body.CreditRating,
                userId: request.Body.UserId,
                leasingContract: request.Body.LeasingContract
            );

            await _companyRepository.AddAsync(company);

            _logger.LogInformation("Successfully created company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
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
            _logger.LogError(ex, "Error creating company for user: {UserId}. CorrelationId: {CorrelationId}",
                request.Body.UserId, request.Header.CorrelationId);
            throw new ApplicationException("Se produjo un error al crear la empresa");
        }
    }
}