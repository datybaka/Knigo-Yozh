using KnigoYozh.Domain.Entities;

namespace KnigoYozh.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}