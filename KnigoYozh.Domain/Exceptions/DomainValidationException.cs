using KnigoYozh.Domain.Exceptions.Abstractions;

namespace KnigoYozh.Domain.Exceptions;
public sealed class DomainValidationException : Exception, IValidationException
{
    public DomainValidationException(string message, IReadOnlyDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
