using FinanceWallet.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinanceWallet.Api.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var (statusCode, errorCode, message) = context.Exception switch
        {
            ValidationException validation =>
                (StatusCodes.Status400BadRequest, "ValidationError",
                    string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))),
            DomainException domain =>
                (StatusCodes.Status400BadRequest, "DomainError", domain.Message),
            NotFoundException notFound =>
                (StatusCodes.Status404NotFound, "NotFound", notFound.Message),
            ConflictException conflict =>
                (StatusCodes.Status409Conflict, "Conflict", conflict.Message),
            ForbiddenException forbidden =>
                (StatusCodes.Status403Forbidden, "Forbidden", forbidden.Message),
            _ => (StatusCodes.Status500InternalServerError, "InternalServerError",
                "An unexpected error occurred.")
        };

        if (statusCode >= 500)
            _logger.LogError(context.Exception, "Unhandled exception during request {Path}", context.HttpContext.Request.Path);
        else
            _logger.LogWarning(context.Exception, "Handled exception during request {Path}", context.HttpContext.Request.Path);

        context.Result = new ObjectResult(new { code = errorCode, message })
        {
            StatusCode = statusCode
        };
        context.ExceptionHandled = true;
    }
}