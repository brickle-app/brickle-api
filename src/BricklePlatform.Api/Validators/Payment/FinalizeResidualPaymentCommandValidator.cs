using BricklePlatform.Api.Application.Commands.Payment;
using BricklePlatform.Api.Validators;
using FluentValidation;

namespace BricklePlatform.Api.Validators.Payment;

public class FinalizeResidualPaymentCommandValidator : AbstractValidator<FinalizeResidualPaymentCommand>
{
    public FinalizeResidualPaymentCommandValidator()
    {
        RuleFor(x => x.Body)
            .NotNull().WithMessage(ValidationMessages.PAYMENT_BODY_REQUIRED);

        When(x => x.Body != null, () =>
        {
            RuleFor(x => x.Body!.UserLeasingAgreementId)
                .NotEmpty().WithMessage(ValidationMessages.PAYMENT_USERLEASINGAGREEMENTID_REQUIRED);
        });
    }
}
