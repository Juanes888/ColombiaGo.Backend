using ColombiaGo.Application.DTOs.Perfil;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class GuardarPreferenciasRequestValidator : AbstractValidator<GuardarPreferenciasRequest>
{
    public GuardarPreferenciasRequestValidator()
    {
        RuleFor(x => x.Categorias).NotNull().Must(c => c.Count <= 4).WithMessage("Máximo 4 categorías.");
        RuleForEach(x => x.Categorias)
            .Must(c => GenerarGuiaViajeRequestValidator.CategoriasValidas.Contains(c))
            .WithMessage("Categoría no válida: usa food, culture, nature o nightlife.");
    }
}
