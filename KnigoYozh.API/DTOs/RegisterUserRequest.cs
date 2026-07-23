using KnigoYozh.Domain.Entities;

namespace KnigoYozh.Api.DTOs;

public record RegisterUserRequest(
    string Email,
    string Username,
    string Password,
    UserRole Role);