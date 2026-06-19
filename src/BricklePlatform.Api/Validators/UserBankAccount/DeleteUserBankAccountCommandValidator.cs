using BricklePlatform.Api.Application.Commands.UserBankAccount;
using FluentValidation;

namespace BricklePlatform.Api.Validators.UserBankAccount;

public class DeleteUserBankAccountCommandValidator : AbstractValidator<DeleteUserBankAccountCommand>
{
    public DeleteUserBankAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la cuenta bancaria es requerido");
    }
}