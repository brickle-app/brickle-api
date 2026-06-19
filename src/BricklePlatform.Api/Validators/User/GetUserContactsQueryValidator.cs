using BricklePlatform.Api.Application.Queries.User;
using FluentValidation;

namespace BricklePlatform.Api.Validators.User;

public class GetUserContactsQueryValidator : AbstractValidator<GetUserContactsQuery>
{
    public GetUserContactsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationMessages.CONTACT_USERID_REQUIRED);
    }
} 