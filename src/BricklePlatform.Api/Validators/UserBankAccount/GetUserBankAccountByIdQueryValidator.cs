using BricklePlatform.Api.Application.Queries.UserBankAccount;
using FluentValidation;

namespace BricklePlatform.Api.Validators.UserBankAccount;

public class GetUserBankAccountByIdQueryValidator : AbstractValidator<GetUserBankAccountByIdQuery>
{
    public GetUserBankAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la cuenta bancaria es requerido");
    }
}