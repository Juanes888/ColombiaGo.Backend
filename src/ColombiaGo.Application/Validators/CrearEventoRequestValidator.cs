using ColombiaGo.Application.DTOs.Eventos;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class CrearEventoRequestValidator : AbstractValidator<CrearEventoRequest>
{
    public CrearEventoRequestValidator()
    {
        RuleFor(x => x.NombreEvento).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DireccionEvento).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Costo).GreaterThanOrEqualTo(0).When(x => x.Costo.HasValue);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
        RuleFor(x => x.LugarId).GreaterThan(0).When(x => x.LugarId.HasValue);
    }
}
