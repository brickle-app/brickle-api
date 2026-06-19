using BricklePlatform.Api.Application.Commands.UserBankAccount;
using BricklePlatform.Api.Validators;
using FluentValidation;

namespace BricklePlatform.Api.Validators.UserBankAccount;

public class CreateUserBankAccountCommandValidator : AbstractValidator<CreateUserBankAccountCommand>
{
    public CreateUserBankAccountCommandValidator()
    {
        RuleFor(x => x.CreateUserBankAccountDto)
            .NotNull().WithMessage("Los datos de la cuenta bancaria son requeridos");

        When(x => x.CreateUserBankAccountDto != null, () =>
        {
            RuleFor(x => x.CreateUserBankAccountDto.UserId)
                .NotEmpty().WithMessage("El ID del usuario es requerido");

            RuleFor(x => x.CreateUserBankAccountDto.BankName)
                .NotEmpty().WithMessage("El nombre del banco es requerido")
                .MaximumLength(100).WithMessage("El nombre del banco no puede exceder 100 caracteres");

            RuleFor(x => x.CreateUserBankAccountDto.AccountType)
                .NotEmpty().WithMessage("El tipo de cuenta es requerido")
                .MaximumLength(50).WithMessage("El tipo de cuenta no puede exceder 50 caracteres");

            RuleFor(x => x.CreateUserBankAccountDto.AccountNumber)
                .NotEmpty().WithMessage("El número de cuenta es requerido")
                .MaximumLength(50).WithMessage("El número de cuenta no puede exceder 50 caracteres");

            RuleFor(x => x.CreateUserBankAccountDto.AccountHolder)
                .NotEmpty().WithMessage("El titular de la cuenta es requerido")
                .MaximumLength(200).WithMessage("El titular de la cuenta no puede exceder 200 caracteres");

            RuleFor(x => x.CreateUserBankAccountDto.AccountDocument)
                .NotEmpty().WithMessage("El documento del titular es requerido")
                .MaximumLength(100).WithMessage("El documento del titular no puede exceder 100 caracteres");

            RuleFor(x => x.CreateUserBankAccountDto.AccountImage)
                .MaximumLength(500).WithMessage("La URL de la imagen no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.CreateUserBankAccountDto.AccountImage));
        });
    }
}