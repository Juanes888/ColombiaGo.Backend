using ColombiaGo.Application.DTOs.Negocio;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class GuardarLugarRequestValidator : AbstractValidator<GuardarLugarRequest>
{
    public GuardarLugarRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category)
            .Must(c => GenerarGuiaViajeRequestValidator.CategoriasValidas.Contains(c))
            .WithMessage("Categoría no válida: usa food, culture, nature o nightlife.");
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Region).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(20).MaximumLength(4000);
        RuleFor(x => x.Tagline).MaximumLength(200);
        RuleFor(x => x.Hours).MaximumLength(150);
        RuleFor(x => x.HowToGet).MaximumLength(150);
        RuleFor(x => x.PriceRange).IsInEnum();
        RuleFor(x => x.EntryCost).GreaterThanOrEqualTo(0).When(x => x.EntryCost is not null);
        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .Must(u => u is null || u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .WithMessage("La imagen debe ser una URL https.");
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m).When(x => x.Latitude is not null);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m).When(x => x.Longitude is not null);
    }
}
