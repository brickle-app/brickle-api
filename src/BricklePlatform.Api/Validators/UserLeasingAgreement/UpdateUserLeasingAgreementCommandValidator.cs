using FluentValidation;
using BricklePlatform.Api.Application.Commands.UserLeasingAgreement;

namespace BricklePlatform.Api.Validators.UserLeasingAgreement;

public class UpdateUserLeasingAgreementCommandValidator : AbstractValidator<UpdateUserLeasingAgreementCommand>
{
    public UpdateUserLeasingAgreementCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_ID_REQUIRED);

        RuleFor(x => x.AgreementDto.RemainingBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_REMAININGBALANCE_GREATER_THAN_OR_EQUAL_TO_ZERO);

        RuleFor(x => x.AgreementDto.EndDate)
            .NotEmpty()
            .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_ENDDATE_REQUIRED)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_ENDDATE_GREATER_THAN_NOW);

        RuleFor(x => x.AgreementDto.Status)
            .NotEmpty()
            .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_STATUS_REQUIRED)
            .MaximumLength(50)
            .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_STATUS_MAXLENGTH);
    }
} 