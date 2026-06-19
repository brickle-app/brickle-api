using BricklePlatform.Api.Application.Commands.UserBankAccount;
using BricklePlatform.Api.Validators;
using FluentValidation;

namespace BricklePlatform.Api.Validators.UserBankAccount;

public class UpdateUserBankAccountCommandValidator : AbstractValidator<UpdateUserBankAccountCommand>
{
    public UpdateUserBankAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la cuenta bancaria es requerido");

        RuleFor(x => x.UpdateUserBankAccountDto)
            .NotNull().WithMessage("Los datos de actualización de la cuenta bancaria son requeridos");

        When(x => x.UpdateUserBankAccountDto != null, () =>
        {
            RuleFor(x => x.UpdateUserBankAccountDto.BankName)
                .MaximumLength(100).WithMessage("El nombre del banco no puede exceder 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.UpdateUserBankAccountDto.BankName));

            RuleFor(x => x.UpdateUserBankAccountDto.AccountType)
                .MaximumLength(50).WithMessage("El tipo de cuenta no puede exceder 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.UpdateUserBankAccountDto.AccountType));

            RuleFor(x => x.UpdateUserBankAccountDto.AccountNumber)
                .MaximumLength(50).WithMessage("El número de cuenta no puede exceder 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.UpdateUserBankAccountDto.AccountNumber));

            RuleFor(x => x.UpdateUserBankAccountDto.AccountHolder)
                .MaximumLength(200).WithMessage("El titular de la cuenta no puede exceder 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.UpdateUserBankAccountDto.AccountHolder));

            RuleFor(x => x.UpdateUserBankAccountDto.AccountDocument)
                .MaximumLength(100).WithMessage("El documento del titular no puede exceder 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.UpdateUserBankAccountDto.AccountDocument));

            RuleFor(x => x.UpdateUserBankAccountDto.AccountImage)
                .MaximumLength(500).WithMessage("La URL de la imagen no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.UpdateUserBankAccountDto.AccountImage));
        });
    }
}