using BricklePlatform.Api.Application.Commands.UserBankAccount;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserBankAccount;

public class UpdateUserBankAccountCommandHandler : IRequestHandler<UpdateUserBankAccountCommand, UserBankAccountDto>
{
    private readonly IUserBankAccountRepository _userBankAccountRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateUserBankAccountCommandHandler> _logger;

    public UpdateUserBankAccountCommandHandler(
        IUserBankAccountRepository userBankAccountRepository,
        ApplicationDbContext context,
        ILogger<UpdateUserBankAccountCommandHandler> logger)
    {
        _userBankAccountRepository = userBankAccountRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<UserBankAccountDto> Handle(UpdateUserBankAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Actualizando cuenta bancaria con ID: {Id} - CorrelationId: {CorrelationId}",
            request.Id, request.Header.CorrelationId);

        try
        {
            var existingAccount = await _context.UserBankAccounts.FindAsync(request.Id);
            if (existingAccount == null)
            {
                _logger.LogWarning("Cuenta bancaria no encontrada para actualizar con ID: {Id} - CorrelationId: {CorrelationId}",
                    request.Id, request.Header.CorrelationId);
                throw new KeyNotFoundException($"Cuenta bancaria con ID {request.Id} no encontrada");
            }

            var updateDto = request.UpdateUserBankAccountDto;

            existingAccount.Update(
                !string.IsNullOrWhiteSpace(updateDto.BankName) ? updateDto.BankName : existingAccount.BankName,
                !string.IsNullOrWhiteSpace(updateDto.AccountType) ? updateDto.AccountType : existingAccount.AccountType,
                !string.IsNullOrWhiteSpace(updateDto.AccountNumber) ? updateDto.AccountNumber : existingAccount.AccountNumber,
                !string.IsNullOrWhiteSpace(updateDto.AccountHolder) ? updateDto.AccountHolder : existingAccount.AccountHolder,
                !string.IsNullOrWhiteSpace(updateDto.AccountDocument) ? updateDto.AccountDocument : existingAccount.AccountDocument,
                !string.IsNullOrWhiteSpace(updateDto.AccountImage) ? updateDto.AccountImage : existingAccount.AccountImage);

            await _userBankAccountRepository.UpdateAsync(existingAccount);

            _logger.LogInformation("Cuenta bancaria actualizada exitosamente con ID: {Id} - CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);

            var updatedAccountDto = new UserBankAccountDto
            {
                Id = existingAccount.Id,
                UserId = existingAccount.UserId,
                BankName = existingAccount.BankName,
                AccountType = existingAccount.AccountType,
                AccountNumber = existingAccount.AccountNumber,
                AccountHolder = existingAccount.AccountHolder,
                AccountDocument = existingAccount.AccountDocument,
                AccountImage = existingAccount.AccountImage,
                CreatedAt = existingAccount.CreatedAt,
                UpdatedAt = existingAccount.UpdatedAt
            };
            
            return updatedAccountDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cuenta bancaria con ID: {Id} - CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);
            throw;
        }
    }
}