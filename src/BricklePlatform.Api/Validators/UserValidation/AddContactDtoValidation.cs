using BricklePlatform.Domain.DTOs;
using FluentValidation;

namespace BricklePlatform.Api.Validators.UserValidation;

public class AddContactDtoValidation : AbstractValidator<AddContactDto>
{
    public AddContactDtoValidation()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty()
            .WithMessage(ValidationMessages.CONTACT_CONTACTID_REQUIRED);
    }
} 