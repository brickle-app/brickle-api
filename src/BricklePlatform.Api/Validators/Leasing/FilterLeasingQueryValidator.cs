using BricklePlatform.Api.Application.Queries.Leasing;
using BricklePlatform.Domain.Enums;
using FluentValidation;

namespace BricklePlatform.Api.Validators.Leasing;

public class FilterLeasingQueryValidator : AbstractValidator<FilterLeasingQuery>
{
    public FilterLeasingQueryValidator()
    {
        RuleFor(x => x.Page)
            .NotNull()
            .WithMessage(ValidationMessages.LEASINGPAGENUMBERREQUIRED)
            .GreaterThan(0)
            .WithMessage(ValidationMessages.LEASINGPAGENUMBERGREATERTHANZERO);

        RuleFor(x => x.Limit)
            .NotNull()
            .WithMessage(ValidationMessages.LEASINGLIMITREQUIRED)
            .GreaterThan(0)
            .WithMessage(ValidationMessages.LEASINGLIMITGREATERTHANZERO)
            .LessThanOrEqualTo(100)
            .WithMessage(ValidationMessages.LEASINGLIMITBETWEENONEANDHUNDRED);

        When(x => x.Categories != null, () =>
        {
            RuleFor(x => x.Categories)
                .Must(categories =>
                {
                    var validCategories = Enum.GetNames(typeof(LeasingTypeEnum));
                    return categories!.All(c => validCategories.Contains(c.ToString()));
                })
                .WithMessage(ValidationMessages.LEASINGINVALIDCATEGORIES);
        });
    }
} 