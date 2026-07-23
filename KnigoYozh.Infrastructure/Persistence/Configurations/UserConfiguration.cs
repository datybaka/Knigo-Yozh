using KnigoYozh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnigoYozh.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // 2. Настройка первичного ключа и конвертация UserId <-> Guid
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,                 // Как писать в БД (достаем Guid)
                value => new UserId(value))     // Как читать из БД (оборачиваем в UserId)
            .IsRequired();

        // 3. Email (Уникальность и лимиты)
        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique(); // База данных на аппаратном уровне не даст вставить дубль

        // 4. Username (Уникальность и лимиты)
        builder.Property(u => u.Username)
            .HasMaxLength(50) // Совпадает с проверкой в Domain-слое (username.Length > 50)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        // 5. PasswordHash
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255) // Хеши обычно фиксированной длины, 255 хватит для BCrypt/Argon2
            .IsRequired();

        // 6. Role (Конвертация Enum в строку)
        builder.Property(u => u.Role)
            .HasConversion<string>() // Сохранит "Admin" или "User" вместо 1 или 0
            .HasMaxLength(20)
            .IsRequired();

        // 7. Даты
        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();
    }
}
