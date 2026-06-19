using FluentValidation;
using BricklePlatform.Api.Application.Commands.Leasing;
using BricklePlatform.Api.Validators;
using System;

namespace BricklePlatform.Api.Validators.Leasing
{
    public class UpdateLeasingCommandValidator : AbstractValidator<UpdateLeasingCommand>
    {
        public UpdateLeasingCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(ValidationMessages.INVALIDGUID);

            RuleFor(x => x.LeasingDto.Name)
                .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD);

            RuleFor(x => x.LeasingDto.Quantity)
                .GreaterThan(0).WithMessage(ValidationMessages.REQUIREDFIELD);

            RuleFor(x => x.LeasingDto.Price)
                .GreaterThan(0).WithMessage(ValidationMessages.REQUIREDFIELD);

            RuleFor(x => x.LeasingDto.Tokens)
                .GreaterThan(0).WithMessage(ValidationMessages.REQUIREDFIELD);

            RuleFor(x => x.LeasingDto.TokensAvailable)
                .GreaterThan(0).WithMessage(ValidationMessages.REQUIREDFIELD);

            RuleFor(x => x.LeasingDto.PricePerToken)
                .GreaterThan(0).WithMessage(ValidationMessages.REQUIREDFIELD);

            RuleFor(x => x.LeasingDto.Description)
                .MaximumLength(200).WithMessage(ValidationMessages.MAXLENGTHFORTEXT)
                .When(x => !string.IsNullOrEmpty(x.LeasingDto.Description));

            RuleFor(x => x.LeasingDto.CoverImageUrl)
                .MaximumLength(255).WithMessage(ValidationMessages.URLMAXLENGTH)
                .When(x => !string.IsNullOrEmpty(x.LeasingDto.CoverImageUrl));

            RuleFor(x => x.LeasingDto.MiniatureImageUrl)
                .MaximumLength(255).WithMessage(ValidationMessages.URLMAXLENGTH)
                .When(x => !string.IsNullOrEmpty(x.LeasingDto.MiniatureImageUrl));

            RuleFor(x => x.LeasingDto.ContractAddress)
                .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD);

            RuleFor(x => x.LeasingDto.ContractTime)
                .Must(BeGreaterOrEqualToNow).WithMessage(ValidationMessages.LEASING_CONTRACTTIME_NOT_BEFORE_NOW)
                .When(x => x.LeasingDto.ContractTime.HasValue);
        }

        private bool BeGreaterOrEqualToNow(DateTime? contractTime)
        {
            return contractTime == null || contractTime.Value >= DateTime.UtcNow.Date;
        }
    }
}