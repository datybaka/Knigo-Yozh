namespace KnigoYozh.Domain.Entities;

public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId Empty => new(Guid.Empty);
}