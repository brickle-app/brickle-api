using BricklePlatform.Api.Application.Commands.Investment;
using FluentValidation;

namespace BricklePlatform.Api.Validators.Investment
{
    public class CreateInvestmentCommandValidator : AbstractValidator<CreateInvestmentCommand>
    {
        public CreateInvestmentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .Must(x => x != Guid.Empty)
                .WithMessage("User ID cannot be empty");

            RuleFor(x => x.LeasingId)
                .NotEmpty()
                .WithMessage("Leasing ID is required")
                .Must(x => x != Guid.Empty)
                .WithMessage("Leasing ID cannot be empty");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero")
                .LessThanOrEqualTo(999999999999.999999m)
                .WithMessage("Amount exceeds maximum allowed value");

            RuleFor(x => x.BricksCount)
                .GreaterThan(0)
                .WithMessage("Bricks count must be greater than zero")
                .LessThanOrEqualTo(int.MaxValue)
                .WithMessage("Bricks count exceeds maximum allowed value");

            RuleFor(x => x.BricksName)
                .NotEmpty()
                .WithMessage("Bricks name is required")
                .MaximumLength(200)
                .WithMessage("Bricks name cannot exceed 200 characters");
        }
    }
}