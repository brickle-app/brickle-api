using BricklePlatform.Api.Application.Commands.User;
using FluentValidation;

namespace BricklePlatform.Api.Validators.User;

public class AddContactCommandValidator : AbstractValidator<AddContactCommand>
{
    public AddContactCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationMessages.CONTACT_USERID_REQUIRED);

        RuleFor(x => x.AddContactDto.ContactId)
            .NotEmpty()
            .WithMessage(ValidationMessages.CONTACT_CONTACTID_REQUIRED);

        RuleFor(x => x)
            .Must(x => x.UserId != x.AddContactDto.ContactId)
            .WithMessage(ValidationMessages.CONTACT_SELF_ADDITION_NOT_ALLOWED);
    }
}