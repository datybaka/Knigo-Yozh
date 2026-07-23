using KnigoYozh.Application.Interfaces;
using KnigoYozh.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KnigoYozh.Application.Users.Queries.GetCurrentUser;

public record UserProfileDto(
    Guid Id,
    string Email,
    string Username,
    string Role,
    DateTime CreatedAtUtc);

public record GetCurrentUserQuery : IRequest<UserProfileDto>;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    private readonly IKnigoYozhDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(
        IKnigoYozhDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id.Value == userId, cancellationToken);

        if (user is null)
            throw new InvalidOperationException("User not found."); // по желанию можно создать NotFoundException

        return new UserProfileDto(
            Id: user.Id.Value,
            Email: user.Email,
            Username: user.Username,
            Role: user.Role.ToString(),
            CreatedAtUtc: user.CreatedAtUtc
        );
    }
}