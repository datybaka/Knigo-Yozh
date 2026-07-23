// KnigoYozh.Application/Interfaces/ICurrentUserService.cs
namespace KnigoYozh.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
}