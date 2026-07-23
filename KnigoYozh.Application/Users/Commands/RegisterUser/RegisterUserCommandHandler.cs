using KnigoYozh.Application.Interfaces;
using KnigoYozh.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KnigoYozh.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Username, string Password, UserRole Role) : IRequest<Guid>;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IKnigoYozhDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IKnigoYozhDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Проверяем, свободен ли Email и Username (Stateful валидация)
        // Используем AnyAsync для максимальной скорости (генерирует SQL EXISTS)
        bool userExists = await _dbContext.Users
            .AnyAsync(u => u.Email == request.Email || u.Username == request.Username, cancellationToken);

        if (userExists)
        {
            // На проде здесь обычно кидают кастомный ConflictException, 
            // который Middleware потом превратит в HTTP 409 Conflict.
            throw new InvalidOperationException("User with this Email or Username already exists.");
        }

        // 2. Хешируем пароль (работает алгоритм Argon2 из инфраструктуры)
        string passwordHash = _passwordHasher.Hash(request.Password);

        // 3. Создаем доменную сущность (отрабатывают все внутренние инварианты)
        var userId = UserId.New();
        var user = new User(
            id: userId,
            email: request.Email,
            username: request.Username,
            passwordHash: passwordHash,
            role: UserRole.User);

        // 4. Добавляем в контекст и сохраняем в БД
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5. Возвращаем сгенерированный Guid (достаем его из Value Object'а)
        return userId.Value;
    }
}

