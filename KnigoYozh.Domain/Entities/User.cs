using KnigoYozh.Domain.Exceptions;
using System.Data;

namespace KnigoYozh.Domain.Entities;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public class User
{
    public UserId Id { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private User() { }

    public User(UserId id, string email, string username, string passwordHash, UserRole role)
    {
        var errors = new Dictionary<string, List<string>>();

        if (id.Value == Guid.Empty)
            AddError(errors, nameof(Id), "Id не может быть пустым.");

        if (!IsValidEmail(email))
            AddError(errors, nameof(Email), "Некорректный Email.");

        if (string.IsNullOrWhiteSpace(username))
            AddError(errors, nameof(Username), "Имя пользователя не может быть пустым.");
        else if (username.Length < 3 || username.Length > 50)
            AddError(errors, nameof(Username), "Имя пользователя должно быть от 3 до 50 символов.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            AddError(errors, nameof(PasswordHash), "Хеш пароля не может быть пустым.");

        if (!Enum.IsDefined(role))
            AddError(errors, nameof(Role), "Указана недопустимая роль.");

        if (errors.Count > 0)
        {
            throw new DomainValidationException(
                "Ошибка создания пользователя.",
                errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()));
        }

        Id = id;
        Email = email;
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private static void AddError(Dictionary<string, List<string>> dict, string key, string message)
    {
        if (!dict.ContainsKey(key))
            dict[key] = new List<string>();
        dict[key].Add(message);
    }

    public void UpdatePassword(string newPasswordHash)
    {
        ValidatePasswordHash(newPasswordHash);
        PasswordHash = newPasswordHash;
    }

    private static void ValidatePasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            var errors = new Dictionary<string, string[]>
        {
            { nameof(PasswordHash), new[] { "Хеш пароля не может быть пустым." } }
        };
            throw new DomainValidationException("Некорректный хеш пароля.", errors);
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Простейшая проверка на наличие '@' и хотя бы одной точки в домене
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
            return false;
        var lastDotIndex = email.LastIndexOf('.');
        return lastDotIndex > atIndex && lastDotIndex < email.Length - 1;
    }
}


