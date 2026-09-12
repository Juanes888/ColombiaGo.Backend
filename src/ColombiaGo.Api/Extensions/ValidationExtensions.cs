using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Extensions;

/// <summary>
/// Runs a FluentValidation validator explicitly inside an action and returns a 400 with a
/// standard <see cref="ValidationProblemDetails"/> body on failure, or null when valid. Manual
/// (rather than an auto-validating action filter) by design — FluentValidation's own docs
/// recommend this since v11; it also keeps "what gets validated" visible at the call site
/// instead of behind reflection-driven filter magic.
/// </summary>
public static class ValidationExtensions
{
    public static async Task<ActionResult?> ValidateAsync<T>(
        this ControllerBase controller, IValidator<T> validator, T instance, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid)
            return null;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return controller.ValidationProblem(new ValidationProblemDetails(errors));
    }
}
