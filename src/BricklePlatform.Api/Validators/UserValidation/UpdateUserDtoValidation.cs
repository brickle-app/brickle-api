using BricklePlatform.Domain.DTOs;
using FluentValidation;

namespace BricklePlatform.Api.Validators.UserValidation;

public class UpdateUserDtoValidation : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidation()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
            .MinimumLength(2).WithMessage(ValidationMessages.NAMEMINLENGTH)
            .MaximumLength(100).WithMessage(ValidationMessages.NAMEMAXLENGTH);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
            .MinimumLength(2).WithMessage(ValidationMessages.NAMEMINLENGTH)
            .MaximumLength(100).WithMessage(ValidationMessages.NAMEMAXLENGTH);

        RuleFor(x => x.ProfilePictureUrl)
            .MaximumLength(255).WithMessage(ValidationMessages.INVALIDURL)
            .When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl));

        RuleFor(x => x.WalletAddress)
            .MaximumLength(42).WithMessage(ValidationMessages.WALLETADDRESSMAXLENGTH)
            .Matches(@"^0x[a-fA-F0-9]{40}$").WithMessage(ValidationMessages.INVALIDWALLETADDRESS)
            .When(x => !string.IsNullOrEmpty(x.WalletAddress));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
            .EmailAddress().WithMessage(ValidationMessages.INVALIDEMAILFORMAT)
            .MaximumLength(100).WithMessage(ValidationMessages.EMAILMAXLENGTH);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
            .MaximumLength(20).WithMessage(ValidationMessages.MAXLENGTH);

        RuleFor(x => x.TermsAccepted)
            .Equal(true).WithMessage(ValidationMessages.REQUIREDFIELD);

        RuleFor(x => x.Nationality)
            .MaximumLength(100).WithMessage(ValidationMessages.MAXLENGTH)
            .When(x => !string.IsNullOrEmpty(x.Nationality));

        RuleFor(x => x.CountryOfResidence)
            .MaximumLength(100).WithMessage(ValidationMessages.MAXLENGTH)
            .When(x => !string.IsNullOrEmpty(x.CountryOfResidence));

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(50).WithMessage(ValidationMessages.MAXLENGTH)
            .When(x => !string.IsNullOrEmpty(x.DocumentNumber));

        RuleFor(x => x.KycCustomerId)
            .MaximumLength(255).WithMessage(ValidationMessages.MAXLENGTH)
            .When(x => !string.IsNullOrEmpty(x.KycCustomerId));

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage(_ => ValidationMessages.INVALIDDOCUMENTTYPE)
            .When(x => x.DocumentType.HasValue);

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
            .When(x => x.DocumentType.HasValue);

        RuleFor(x => x.CurrentSession)
            .MaximumLength(4000).WithMessage(ValidationMessages.CURRENTSESSIONMAXLENGTH)
            .When(x => !string.IsNullOrEmpty(x.CurrentSession));

        RuleFor(x => x.ExternalWalletId)
            .MaximumLength(255).WithMessage(ValidationMessages.MAXLENGTH)
            .When(x => !string.IsNullOrEmpty(x.ExternalWalletId));
    }
}