using KnigoYozh.Domain.Exceptions.Abstractions;

namespace KnigoYozh.Application.Exceptions;

public sealed class ValidationException : Exception, IValidationException
{
    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
