using ColombiaGo.Application.DTOs.Admin;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class CrearReporteRequestValidator : AbstractValidator<CrearReporteRequest>
{
    public CrearReporteRequestValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Formato).IsInEnum();
        RuleFor(x => x.FechaFin)
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha fin debe ser igual o posterior a la fecha inicio.");
    }
}
