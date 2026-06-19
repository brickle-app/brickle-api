using BricklePlatform.Api.Application.Commands.UserBankAccount;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.Entities;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserBankAccount;

public class CreateUserBankAccountCommandHandler : IRequestHandler<CreateUserBankAccountCommand, UserBankAccountDto>
{
    private readonly IUserBankAccountRepository _userBankAccountRepository;
    private readonly ILogger<CreateUserBankAccountCommandHandler> _logger;

    public CreateUserBankAccountCommandHandler(
        IUserBankAccountRepository userBankAccountRepository,
        ILogger<CreateUserBankAccountCommandHandler> logger)
    {
        _userBankAccountRepository = userBankAccountRepository;
        _logger = logger;
    }

    public async Task<UserBankAccountDto> Handle(CreateUserBankAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creando nueva cuenta bancaria para usuario: {UserId} - CorrelationId: {CorrelationId}",
            request.CreateUserBankAccountDto.UserId, request.Header.CorrelationId);

        try
        {
            var userBankAccount = BricklePlatform.Domain.Entities.UserBankAccount.Create(
                request.CreateUserBankAccountDto.UserId,
                request.CreateUserBankAccountDto.BankName,
                request.CreateUserBankAccountDto.AccountType,
                request.CreateUserBankAccountDto.AccountNumber,
                request.CreateUserBankAccountDto.AccountHolder,
                request.CreateUserBankAccountDto.AccountDocument,
                request.CreateUserBankAccountDto.AccountImage);

            await _userBankAccountRepository.CreateAsync(userBankAccount);

            _logger.LogInformation("Cuenta bancaria creada exitosamente con ID: {Id} para usuario: {UserId} - CorrelationId: {CorrelationId}",
                userBankAccount.Id, request.CreateUserBankAccountDto.UserId, request.Header.CorrelationId);

            var createdAccountDto = new UserBankAccountDto
            {
                Id = userBankAccount.Id,
                UserId = userBankAccount.UserId,
                BankName = userBankAccount.BankName,
                AccountType = userBankAccount.AccountType,
                AccountNumber = userBankAccount.AccountNumber,
                AccountHolder = userBankAccount.AccountHolder,
                AccountDocument = userBankAccount.AccountDocument,
                AccountImage = userBankAccount.AccountImage,
                CreatedAt = userBankAccount.CreatedAt,
                UpdatedAt = userBankAccount.UpdatedAt
            };
            
            return createdAccountDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cuenta bancaria para usuario: {UserId} - CorrelationId: {CorrelationId}",
                request.CreateUserBankAccountDto.UserId, request.Header.CorrelationId);
            throw;
        }
    }
}