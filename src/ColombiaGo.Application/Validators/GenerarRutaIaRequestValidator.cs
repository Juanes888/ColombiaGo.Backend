using ColombiaGo.Application.DTOs.Rutas;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class GenerarRutaIaRequestValidator : AbstractValidator<GenerarRutaIaRequest>
{
    public GenerarRutaIaRequestValidator()
    {
        RuleFor(x => x.LugarSlugs).NotEmpty().WithMessage("Elige al menos un lugar.");
        RuleFor(x => x.LugarSlugs.Count).LessThanOrEqualTo(20).WithMessage("Elige como máximo 20 lugares.");
        RuleFor(x => x.Presupuesto).GreaterThan(0).WithMessage("El presupuesto debe ser mayor a 0.");
    }
}
