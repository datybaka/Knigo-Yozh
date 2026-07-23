using Isopoh.Cryptography.Argon2;
using KnigoYozh.Application.Interfaces;

namespace KnigoYozh.Infrastructure.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return Argon2.Hash(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return Argon2.Verify(passwordHash, password);
    }
}
