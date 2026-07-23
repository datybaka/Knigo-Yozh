using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KnigoYozh.Domain.Exceptions;
using KnigoYozh.Domain.Exceptions.Abstractions;

namespace KnigoYozh.Api.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Логируем только критические/системные ошибки. 
        // Доменные и входные ошибки не замусоривают ERROR-лог.
        if (IsExpectedException(exception))
        {
            _logger.LogWarning("Обработана ожидаемая ошибка ({ExceptionType}): {Message}",
                exception.GetType().Name, exception.Message);
        }
        else
        {
            _logger.LogError(exception, "Произошла непредвиденная ошибка: {Message}", exception.Message);
        }

        // 2. Определяем HTTP-статус
        var statusCode = exception switch
        {
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            DomainValidationException => StatusCodes.Status400BadRequest,
            IValidationException => StatusCodes.Status422UnprocessableEntity,
            INotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict, // Например, Email/Username занят
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        // 3. Формируем ProblemDetails
        ProblemDetails problemDetails = exception switch
        {
            // А) Ошибки входных данных (FluentValidation)
            FluentValidation.ValidationException fluentEx => new ValidationProblemDetails(
                fluentEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Title = "Ошибка валидации входных данных",
                Detail = "Одно или несколько полей запроса содержат некорректные данные.",
                Status = statusCode
            },

            // Б) Ошибки валидации внутри Домена (User.cs)
            DomainValidationException domainValEx => new ValidationProblemDetails((IDictionary<string, string[]>)domainValEx.Errors)
            {
                Title = "Ошибка доменной валидации",
                Detail = domainValEx.Message,
                Status = statusCode
            },

            // В) Кастомные IValidationException
            IValidationException validationException => new ValidationProblemDetails(
                (IDictionary<string, string[]>)validationException.Errors)
            {
                Title = "Validation Error",
                Detail = validationException.Message,
                Status = statusCode
            },

            // Г) Ресурс не найден
            INotFoundException notFoundException => new ProblemDetails
            {
                Title = "Resource Not Found",
                Detail = notFoundException.Message,
                Status = statusCode
            },

            // Д) Бизнес-конфликты (Дубликаты сущностей)
            InvalidOperationException invalidOpEx => new ProblemDetails
            {
                Title = "Conflict",
                Detail = invalidOpEx.Message,
                Status = statusCode
            },

            // Е) Непредвиденные системные ошибки
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

    private static bool IsExpectedException(Exception exception) =>
        exception is INotFoundException
            or IValidationException
            or FluentValidation.ValidationException
            or DomainValidationException
            or InvalidOperationException;
}