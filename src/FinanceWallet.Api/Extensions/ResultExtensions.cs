using FinanceWallet.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace FinanceWallet.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result, Func<T, object> mapper) =>
        result.IsSuccess
            ? controller.Ok(mapper(result.Value!))
            : result.Error!.ToActionResult();

    public static IActionResult ToActionResult(this ControllerBase controller, Result result) =>
        result.IsSuccess
            ? controller.NoContent()
            : result.Error!.ToActionResult();

    private static IActionResult ToActionResult(this Error error) =>
        new ObjectResult(new { error.Code, error.Message })
        {
            StatusCode = error.StatusCode
        };
}