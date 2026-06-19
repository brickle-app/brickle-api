using BricklePlatform.Domain.DTOs;
using FluentValidation;

namespace BricklePlatform.Api.Validators.CompanyValidation
{
    public class UpdateCompanyDtoValidation : AbstractValidator<UpdateCompanyDto>
    {
        public UpdateCompanyDtoValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessages.COMPANYNAMEREQUIRED)
                .MaximumLength(200).WithMessage(ValidationMessages.COMPANYNAMEMAXLENGTH);

            RuleFor(x => x.OperationTime)
                .GreaterThan(0).WithMessage(ValidationMessages.OPERATIONTIMEGREATERTHANZERO);

            RuleFor(x => x.OperationMeasure)
                .NotEmpty().WithMessage(ValidationMessages.OPERATIONMEASUREREQUIRED)
                .Must(x => x.ToLower() == "monthly" || x.ToLower() == "yearly")
                .WithMessage(ValidationMessages.OPERATIONMEASUREVALID);

            RuleFor(x => x.CreditRating)
                .NotEmpty().WithMessage(ValidationMessages.CREDITRATINGREQUIRED)
                .MaximumLength(50).WithMessage(ValidationMessages.CREDITRATINGMAXLENGTH);

            RuleFor(x => x.LeasingContract)
                .MaximumLength(500).WithMessage(ValidationMessages.LEASINGCONTRACTMAXLENGTH)
                .When(x => !string.IsNullOrEmpty(x.LeasingContract));
        }
    }
}