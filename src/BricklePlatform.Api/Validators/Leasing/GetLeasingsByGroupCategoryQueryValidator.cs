using BricklePlatform.Api.Application.Queries.Leasing;
using FluentValidation;

namespace BricklePlatform.Api.Validators.Leasing;

public class GetLeasingsByGroupCategoryQueryValidator : AbstractValidator<GetLeasingsByGroupCategoryQuery>
{
    public GetLeasingsByGroupCategoryQueryValidator()
    {
        RuleFor(x => x.GroupCategory)
            .IsInEnum()
            .WithMessage("La categoría de grupo especificada no es válida");
    }
}