using BricklePlatform.Api.Application.Commands.UserBankAccount;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserBankAccount;

public class DeleteUserBankAccountCommandHandler : IRequestHandler<DeleteUserBankAccountCommand, bool>
{
    private readonly IUserBankAccountRepository _userBankAccountRepository;
    private readonly ILogger<DeleteUserBankAccountCommandHandler> _logger;

    public DeleteUserBankAccountCommandHandler(
        IUserBankAccountRepository userBankAccountRepository,
        ILogger<DeleteUserBankAccountCommandHandler> logger)
    {
        _userBankAccountRepository = userBankAccountRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteUserBankAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Eliminando cuenta bancaria con ID: {Id} - CorrelationId: {CorrelationId}",
            request.Id, request.Header.CorrelationId);

        try
        {
            var exists = await _userBankAccountRepository.ExistsAsync(request.Id);
            if (!exists)
            {
                _logger.LogWarning("Cuenta bancaria no encontrada para eliminar con ID: {Id} - CorrelationId: {CorrelationId}",
                    request.Id, request.Header.CorrelationId);
                return false;
            }

            var result = await _userBankAccountRepository.DeleteAsync(request.Id);

            if (result)
            {
                _logger.LogInformation("Cuenta bancaria eliminada exitosamente con ID: {Id} - CorrelationId: {CorrelationId}",
                    request.Id, request.Header.CorrelationId);
            }
            else
            {
                _logger.LogWarning("No se pudo eliminar la cuenta bancaria con ID: {Id} - CorrelationId: {CorrelationId}",
                    request.Id, request.Header.CorrelationId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cuenta bancaria con ID: {Id} - CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);
            throw;
        }
    }
}