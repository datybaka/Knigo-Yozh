using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KnigoYozh.Domain.Exceptions.Abstractions;

namespace KnigoYozh.Api.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> _logger,
        IProblemDetailsService problemDetailsService)
    {
        this._logger = _logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Логируем только критические/системные ошибки
        if (exception is not INotFoundException and not IValidationException)
        {
            _logger.LogError(exception, "Произошла непредвиденная ошибка: {Message}", exception.Message);
        }

        // 2. Определяем статус-код
        var statusCode = exception switch
        {
            INotFoundException => StatusCodes.Status404NotFound,
            IValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        // 3. Формируем ProblemDetails
        ProblemDetails problemDetails = exception switch
        {
            IValidationException validationException => new ValidationProblemDetails((IDictionary<string, string[]>)validationException.Errors)
            {
                Title = "Validation Error",
                Detail = validationException.Message,
                Status = statusCode
            },
            INotFoundException notFoundException => new ProblemDetails
            {
                Title = "Resource Not Found",
                Detail = notFoundException.Message,
                Status = statusCode
            },
            _ => new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Status = statusCode
            }
        };

        // 4. Пишем ответ в поток через ProblemDetailsService
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}