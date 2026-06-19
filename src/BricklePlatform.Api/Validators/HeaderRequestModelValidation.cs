using BricklePlatform.Api.Models;
using FluentValidation;

namespace BricklePlatform.Api.Validators
{
    public class HeaderRequestModelValidation : AbstractValidator<HeaderRequestModel>
    {
        public HeaderRequestModelValidation()
        {
            RuleFor(c => c.CorrelationId)
                .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
                .Must(GuidValidator.IsGuid).WithMessage(ValidationMessages.INVALIDGUID);
            RuleFor(c => c.User)
                .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
                .MaximumLength(50).WithMessage(ValidationMessages.MAXLENGTH);
            RuleFor(c => c.Source)
                .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
                .MaximumLength(50).WithMessage(ValidationMessages.MAXLENGTH);
            RuleFor(c => c.RequestDate)
                .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD);
        }
    }
}