using BricklePlatform.Api.Application.Queries.UserBankAccount;
using FluentValidation;

namespace BricklePlatform.Api.Validators.UserBankAccount;

public class GetUserBankAccountsQueryValidator : AbstractValidator<GetUserBankAccountsQuery>
{
    public GetUserBankAccountsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido");
    }
}