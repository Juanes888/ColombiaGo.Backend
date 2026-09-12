using ColombiaGo.Application.DTOs.Admin;
using FluentValidation;

namespace ColombiaGo.Application.Validators;

public sealed class ModerarLugarRequestValidator : AbstractValidator<ModerarLugarRequest>
{
    public ModerarLugarRequestValidator()
    {
        RuleFor(x => x.MotivoRechazo)
            .NotEmpty()
            .When(x => !x.Aprobar)
            .WithMessage("Debes indicar un motivo de rechazo.");
    }
}
