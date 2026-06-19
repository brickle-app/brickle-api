using BricklePlatform.Api.Application.Queries.File;
using FluentValidation;

namespace BricklePlatform.Api.Validators.File;

public class GetFileQueryValidator : AbstractValidator<GetFileQuery>
{
    public GetFileQueryValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("El tipo de entidad es requerido")
            .WithMessage("Tipo de entidad no válido");

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("El ID de la entidad es requerido");
    }
}