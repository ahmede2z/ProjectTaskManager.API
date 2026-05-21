using Microsoft.AspNetCore.Mvc;
using ProjectTaskManager.Application.Common.Models;

namespace ProjectTaskManager.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return result.Error!.Type switch
        {
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.NotFound => NotFound(result.Error),
            ErrorType.Forbidden => Forbidden(result.Error),
            ErrorType.Unauthorized => Unauthorized(result.Error),
            ErrorType.Conflict => Conflict(result.Error),
            _ => InternalServerError(result.Error)
        };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return result.Error!.Type switch
        {
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.NotFound => NotFound(result.Error),
            ErrorType.Forbidden => Forbidden(result.Error),
            ErrorType.Unauthorized => Unauthorized(result.Error),
            ErrorType.Conflict => Conflict(result.Error),
            _ => InternalServerError(result.Error)
        };
    }

    public static IActionResult ToCreatedAtAction<T>(
        this Result<T> result,
        string actionName,
        object? routeValues = null)
    {
        if (result.IsSuccess)
            return new CreatedAtActionResult(actionName, null, routeValues, result.Value);

        return result.ToActionResult();
    }

    public static IActionResult ToCreatedAtAction<T>(
        this Result<T> result,
        string actionName,
        Func<T, object> routeValuesSelector)
    {
        if (result.IsSuccess)
            return new CreatedAtActionResult(actionName, null, routeValuesSelector(result.Value!), result.Value);

        return result.ToActionResult();
    }

    private static ObjectResult BadRequest(Error error) =>
        new ObjectResult(ToProblemDetails(error, StatusCodes.Status400BadRequest))
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

    private static ObjectResult NotFound(Error error) =>
        new ObjectResult(ToProblemDetails(error, StatusCodes.Status404NotFound))
        {
            StatusCode = StatusCodes.Status404NotFound
        };

    private static ObjectResult Forbidden(Error error) =>
        new ObjectResult(ToProblemDetails(error, StatusCodes.Status403Forbidden))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

    private static ObjectResult Unauthorized(Error error) =>
        new ObjectResult(ToProblemDetails(error, StatusCodes.Status401Unauthorized))
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };

    private static ObjectResult Conflict(Error error) =>
        new ObjectResult(ToProblemDetails(error, StatusCodes.Status409Conflict))
        {
            StatusCode = StatusCodes.Status409Conflict
        };

    private static ObjectResult InternalServerError(Error error) =>
        new ObjectResult(ToProblemDetails(error, StatusCodes.Status500InternalServerError))
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

    private static ProblemDetails ToProblemDetails(Error error, int status) =>
        new()
        {
            Title = error.Code,
            Detail = error.Message,
            Status = status
        };
}
