namespace KnigoYozh.Domain.Exceptions.Abstractions;

public interface IValidationException
{
    string Message { get; }
    IReadOnlyDictionary<string, string[]> Errors { get; }
}
