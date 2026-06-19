using FluentValidation;
using BricklePlatform.Api.Application.Commands.UserLeasingAgreement;

namespace BricklePlatform.Api.Validators.UserLeasingAgreement
{
    public class CreateUserLeasingAgreementCommandValidator : AbstractValidator<CreateUserLeasingAgreementCommand>
    {
        public CreateUserLeasingAgreementCommandValidator()
        {
            RuleFor(x => x.Agreement.UserId)
                .NotEmpty()
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_USERID_REQUIRED);

            RuleFor(x => x.Agreement.LeasingId)
                .NotEmpty()
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_LEASINGID_REQUIRED);

            RuleFor(x => x.Agreement.PaymentTerm)
                .NotEmpty()
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_PAYMENTTERM_REQUIRED)
                .MaximumLength(50)
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_PAYMENTTERM_MAXLENGTH);

            RuleFor(x => x.Agreement.ContractDetails)
                .NotEmpty()
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_CONTRACTDETAILS_REQUIRED)
                .MaximumLength(500)
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_CONTRACTDETAILS_MAXLENGTH);

            RuleFor(x => x.Agreement.StartDate)
                .NotEmpty()
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_STARTDATE_REQUIRED)
                .LessThan(x => x.Agreement.EndDate)
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_STARTDATE_BEFORE_ENDDATE);

            RuleFor(x => x.Agreement.EndDate)
                .NotEmpty()
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_ENDDATE_REQUIRED)
                .GreaterThan(x => x.Agreement.StartDate)
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_ENDDATE_AFTER_STARTDATE);

            RuleFor(x => x.Agreement.ResidualValue)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_RESIDUAL_VALUE_GREATERTHANZERO);

            RuleFor(x => x.Agreement.LeasingCoreAddress)
                .NotEmpty()
                .WithMessage(ValidationMessages.USERLEASINGAGREEMENT_LEASING_ADDRESS_REQUIRED);
        }
    }
}