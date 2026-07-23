using KnigoYozh.Application.Interfaces;
using KnigoYozh.Domain.Entities;
using KnigoYozh.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;

namespace KnigoYozh.Application.Users.Commands.LoginUser;

public record LoginResponse(string Token, string Email, string Username, string Role);

public record LoginUserCommand(string Email, string Password) : IRequest<LoginResponse>;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IKnigoYozhDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        IKnigoYozhDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Ищем пользователя по email
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        // 2. Проверяем пароль через Argon2id
        bool passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        // 3. Генерируем JWT
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 4. Возвращаем DTO
        return new LoginResponse(
            Token: token,
            Email: user.Email,
            Username: user.Username,
            Role: user.Role.ToString()
        );
    }
}