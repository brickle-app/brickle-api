using BricklePlatform.Api.Application.Queries.UserBankAccount;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserBankAccount;

public class GetUserBankAccountsQueryHandler : IRequestHandler<GetUserBankAccountsQuery, IEnumerable<UserBankAccountSummaryDto>>
{
    private readonly IUserBankAccountRepository _userBankAccountRepository;
    private readonly ILogger<GetUserBankAccountsQueryHandler> _logger;

    public GetUserBankAccountsQueryHandler(
        IUserBankAccountRepository userBankAccountRepository,
        ILogger<GetUserBankAccountsQueryHandler> logger)
    {
        _userBankAccountRepository = userBankAccountRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserBankAccountSummaryDto>> Handle(GetUserBankAccountsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo cuentas bancarias para usuario: {UserId} - CorrelationId: {CorrelationId}",
            request.UserId, request.Header.CorrelationId);

        try
        {
            var bankAccounts = await _userBankAccountRepository.GetSummaryByUserIdAsync(request.UserId);
            
            _logger.LogInformation("Se encontraron {Count} cuentas bancarias para usuario: {UserId} - CorrelationId: {CorrelationId}",
                bankAccounts.Count(), request.UserId, request.Header.CorrelationId);

            return bankAccounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas bancarias para usuario: {UserId} - CorrelationId: {CorrelationId}",
                request.UserId, request.Header.CorrelationId);
            throw;
        }
    }
}