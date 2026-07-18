namespace KnigoYozh.Domain.Exceptions.Abstractions;

public interface IValidationException
{
    string Message { get; }
    // Словарь: Поле -> Список ошибок к этому полю
    IReadOnlyDictionary<string, string[]> Errors { get; }
}
