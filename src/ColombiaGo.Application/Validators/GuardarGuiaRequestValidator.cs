using ColombiaGo.Application.DTOs.Guias;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class GuardarGuiaRequestValidator : AbstractValidator<GuardarGuiaRequest>
{
    public GuardarGuiaRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specialty).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Place).NotEmpty().MaximumLength(150);
        RuleFor(x => x.HourlyRate).GreaterThan(0).LessThanOrEqualTo(10_000_000m);
        RuleFor(x => x.Languages).NotEmpty().Must(l => l.Count <= 10).WithMessage("Máximo 10 idiomas.");
        RuleForEach(x => x.Languages).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Bio).MaximumLength(1000);
        RuleFor(x => x.ExperienceYears).InclusiveBetween(0, 70);
        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500)
            .Must(u => u is null || u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .WithMessage("La foto debe ser una URL https.");
    }
}
