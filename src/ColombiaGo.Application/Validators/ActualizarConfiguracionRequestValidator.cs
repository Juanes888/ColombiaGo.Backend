using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Perfil;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class ActualizarConfiguracionRequestValidator : AbstractValidator<ActualizarConfiguracionRequest>
{
    public ActualizarConfiguracionRequestValidator()
    {
        RuleFor(x => x.Language)
            .Must(Idiomas.EsSoportado)
            .When(x => x.Language is not null)
            .WithMessage("Idioma no soportado: usa es, en, pt, zh o fr.");
    }
}
