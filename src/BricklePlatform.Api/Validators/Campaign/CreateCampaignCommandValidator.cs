using BricklePlatform.Api.Application.Commands.Campaign;
using FluentValidation;

namespace BricklePlatform.Api.Validators;

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.TokenizeAsset)
            .NotNull()
            .WithMessage("Campaign y Leasing son requeridos.");

        When(x => x.TokenizeAsset != null, () =>
        {
            RuleFor(x => x.TokenizeAsset!.Campaign)
                .NotNull()
                .WithMessage("Los datos de la campaña son requeridos.");

            RuleFor(x => x.TokenizeAsset!.Leasing)
                .NotNull()
                .WithMessage("Los datos del acuerdo de leasing son requeridos.");
        });

        When(x => x.TokenizeAsset?.Campaign != null, () =>
        {
            RuleFor(x => x.TokenizeAsset!.Campaign.LeasingId)
                .NotEmpty()
                .WithMessage("El LeasingId es requerido.");
            RuleFor(x => x.TokenizeAsset!.Campaign.MinCapital)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El capital mínimo no puede ser negativo.");
            RuleFor(x => x.TokenizeAsset!.Campaign.MaxCapital)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El capital máximo no puede ser negativo.");
            RuleFor(x => x.TokenizeAsset!.Campaign)
                .Must(c => c.MinCapital <= c.MaxCapital)
                .WithMessage("El capital mínimo no puede ser mayor al capital máximo.")
                .When(x => x.TokenizeAsset!.Campaign.MinCapital >= 0 && x.TokenizeAsset.Campaign.MaxCapital >= 0);
            RuleFor(x => x.TokenizeAsset!.Campaign.BaseToken)
                .NotEmpty()
                .WithMessage("La dirección del Base Token es requerida.");
            RuleFor(x => x.TokenizeAsset!.Campaign.BrickleAddress)
                .NotEmpty()
                .WithMessage("La dirección de Brickle es requerida.");
        });

        When(x => x.TokenizeAsset?.Leasing != null, () =>
        {
            RuleFor(x => x.TokenizeAsset!.Leasing.UserId)
                .NotEmpty()
                .WithMessage("Leasing.UserId es requerido.");
            RuleFor(x => x.TokenizeAsset!.Leasing.LeasingId)
                .NotEmpty()
                .WithMessage("Leasing.LeasingId es requerido.");
            RuleFor(x => x.TokenizeAsset!.Leasing.AssetValue)
                .GreaterThan(0)
                .WithMessage("Leasing.AssetValue debe ser mayor a 0.");
            RuleFor(x => x.TokenizeAsset!.Leasing.UsefulLife)
                .GreaterThan(0)
                .WithMessage("Leasing.UsefulLife debe ser mayor a 0 (años).");
            RuleFor(x => x.TokenizeAsset!.Leasing.TermTime)
                .GreaterThan(0)
                .WithMessage("Leasing.TermTime debe ser mayor a 0 (meses).");
        });
    }
}