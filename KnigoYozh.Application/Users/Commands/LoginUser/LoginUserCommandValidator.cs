using FluentValidation;

namespace KnigoYozh.Application.Users.Commands.LoginUser;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email необходим.")
            .EmailAddress().WithMessage("Некорректный email формат.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль необходим.");
    }
}