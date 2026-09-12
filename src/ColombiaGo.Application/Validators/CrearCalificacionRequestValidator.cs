using ColombiaGo.Application.DTOs.Calificaciones;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class CrearCalificacionRequestValidator : AbstractValidator<CrearCalificacionRequest>
{
    public CrearCalificacionRequestValidator()
    {
        RuleFor(x => x.Puntuacion).InclusiveBetween(1, 5);
        RuleFor(x => x.Comentario).MaximumLength(1000);
    }
}
