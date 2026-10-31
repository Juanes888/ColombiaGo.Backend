using ColombiaGo.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Extensions;

/// <summary>
/// Translates an Application-layer <see cref="Result"/>/<see cref="Result{T}"/> into the matching
/// HTTP response. Every controller action that returns a Result goes through here, so the
/// mapping from <see cref="ErrorType"/> to status code only lives in one place.
/// </summary>
public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, Result<T> result) =>
        result.IsSuccess ? controller.Ok(result.Value) : controller.Problem(result.Error);

    public static ActionResult ToActionResult(this ControllerBase controller, Result result) =>
        result.IsSuccess ? controller.NoContent() : controller.Problem(result.Error);

    /// <summary>Same as <see cref="ToActionResult{T}"/> but returns 201 Created on success.</summary>
    public static ActionResult<T> ToCreatedResult<T>(
        this ControllerBase controller, Result<T> result, string actionName, object routeValues) =>
        result.IsSuccess
            ? controller.CreatedAtAction(actionName, routeValues, result.Value)
            : controller.Problem(result.Error);

    private static ObjectResult Problem(this ControllerBase controller, Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status500InternalServerError,
        };

        return controller.Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
    }
}
