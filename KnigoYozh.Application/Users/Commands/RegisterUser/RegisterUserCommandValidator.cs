using FluentValidation;
using KnigoYozh.Domain.Entities;

namespace KnigoYozh.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        // 1. Валидация Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email не может быть пустым.")
            .EmailAddress().WithMessage("Некорректный формат Email.");

        // 2. Валидация Username
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Имя пользователя не может быть пустым.")
            .MinimumLength(3).WithMessage("Имя пользователя должно быть не менее 3 символов.")
            .MaximumLength(50).WithMessage("Имя пользователя не должно превышать 50 символов.");

        // 3. Валидация сырого Пароля
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль не может быть пустым.")
            .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов.")
            .Matches(@"[a-zA-Z]").WithMessage("Пароль должен содержать хотя бы одну букву.")
            .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Пароль должен содержать хотя бы один спецсимвол.");

        // 4. Валидация Роли
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Указана недопустимая роль.");
    }
}
