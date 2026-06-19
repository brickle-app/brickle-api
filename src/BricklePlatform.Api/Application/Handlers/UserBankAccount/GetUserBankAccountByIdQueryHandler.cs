using BricklePlatform.Api.Application.Queries.UserBankAccount;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserBankAccount;

public class GetUserBankAccountByIdQueryHandler : IRequestHandler<GetUserBankAccountByIdQuery, UserBankAccountDto?>
{
    private readonly IUserBankAccountRepository _userBankAccountRepository;
    private readonly ILogger<GetUserBankAccountByIdQueryHandler> _logger;

    public GetUserBankAccountByIdQueryHandler(
        IUserBankAccountRepository userBankAccountRepository,
        ILogger<GetUserBankAccountByIdQueryHandler> logger)
    {
        _userBankAccountRepository = userBankAccountRepository;
        _logger = logger;
    }

    public async Task<UserBankAccountDto?> Handle(GetUserBankAccountByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo cuenta bancaria por ID: {Id} - CorrelationId: {CorrelationId}",
            request.Id, request.Header.CorrelationId);

        try
        {
            var bankAccount = await _userBankAccountRepository.GetByIdAsync(request.Id);
            
            if (bankAccount == null)
            {
                _logger.LogWarning("Cuenta bancaria no encontrada con ID: {Id} - CorrelationId: {CorrelationId}",
                    request.Id, request.Header.CorrelationId);
                return null;
            }

            _logger.LogInformation("Cuenta bancaria encontrada con ID: {Id} - CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);

            return bankAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuenta bancaria por ID: {Id} - CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);
            throw;
        }
    }
}