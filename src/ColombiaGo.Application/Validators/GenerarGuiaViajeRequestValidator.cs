using ColombiaGo.Application.DTOs.GuiaViaje;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class GenerarGuiaViajeRequestValidator : AbstractValidator<GenerarGuiaViajeRequest>
{
    public static readonly IReadOnlyList<string> CategoriasValidas = ["food", "culture", "nature", "nightlife"];

    public GenerarGuiaViajeRequestValidator()
    {
        RuleFor(x => x.Ciudad).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Dias).InclusiveBetween(1, 14);
        RuleFor(x => x.Presupuesto).GreaterThan(0).LessThanOrEqualTo(1_000_000_000m);
        RuleFor(x => x.Intereses)
            .Must(i => i is null || i.Count <= CategoriasValidas.Count)
            .WithMessage("Demasiados intereses.");
        RuleForEach(x => x.Intereses)
            .Must(i => CategoriasValidas.Contains(i))
            .WithMessage("Interés no válido: usa food, culture, nature o nightlife.");
    }
}
