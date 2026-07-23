using KnigoYozh.Domain.Entities;
using KnigoYozh.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using KnigoYozh.Infrastructure.Persistence;

namespace KnigoYozh.Tests.Tests;

public class UserPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private KnigoYozhDbContext _dbContext = null!;

    public UserPersistenceTests()
    {
        // Образ PostgreSQL, порт – любой свободный
        _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("testpass")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // 1. Запускаем контейнер
        await _dbContainer.StartAsync();

        // 2. Формируем строку подключения
        var connectionString = _dbContainer.GetConnectionString();

        // 3. Создаём DbContext и накатываем миграции
        var options = new DbContextOptionsBuilder<KnigoYozhDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        _dbContext = new KnigoYozhDbContext(options);

        // Применяем миграции (убедитесь, что они скомпилированы в сборке Infrastructure)
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
            await _dbContext.DisposeAsync();

        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task InsertAndRetrieve_User_ShouldPreserveAllProperties()
    {
        // Arrange
        var userId = UserId.New();
        var user = new User(
            id: userId,
            email: "ivan@example.com",
            username: "ivanov",
            passwordHash: "some_hashed_password",
            role: UserRole.Admin);

        // Act: сохраняем
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Отсоединяем сущность, чтобы следующий запрос точно прочитал из БД
        _dbContext.Entry(user).State = EntityState.Detached;

        var retrieved = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == userId);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(userId, retrieved.Id);                         // UserId ↔ Guid
        Assert.Equal("ivan@example.com", retrieved.Email);
        Assert.Equal("ivanov", retrieved.Username);
        Assert.Equal("some_hashed_password", retrieved.PasswordHash);
        Assert.Equal(UserRole.Admin, retrieved.Role);               // enum → строка → enum
        Assert.True((DateTime.UtcNow - retrieved.CreatedAtUtc).Duration() < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DuplicateEmail_ShouldThrowDbUpdateException()
    {
        // Arrange
        var user1 = new User(UserId.New(), "dup@example.com", "user1", "hash", UserRole.User);
        _dbContext.Users.Add(user1);
        await _dbContext.SaveChangesAsync();

        var user2 = new User(UserId.New(), "dup@example.com", "user2", "hash", UserRole.User);
        _dbContext.Users.Add(user2);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task DuplicateUsername_ShouldThrowDbUpdateException()
    {
        // Arrange
        var u1 = new User(UserId.New(), "a@b.c", "sameusername", "hash", UserRole.User);
        _dbContext.Users.Add(u1);
        await _dbContext.SaveChangesAsync();

        var u2 = new User(UserId.New(), "d@e.f", "sameusername", "hash", UserRole.User);
        _dbContext.Users.Add(u2);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task InsertUser_WithEmptyId_ShouldThrowDomainException()
    {
        // Проверяем, что доменная валидация работает, даже если попытаемся обойти её (на уровне БД мы бы получили ошибку,
        // но здесь убедимся, что конструктор бросает исключение)
        Assert.Throws<DomainValidationException>(() =>
            new User(UserId.Empty, "valid@mail.com", "user", "hash", UserRole.User));
    }

    [Fact]
    public async Task DirectSqlCheck_UserId_IsStoredAsUuid()
    {
        var userId = UserId.New();
        var user = new User(userId, "sql@test.com", "sqltest", "hash", UserRole.User);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Получаем сырое значение Id из базы через обычный ADO.NET
        var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT \"Id\" FROM users WHERE \"Username\" = 'sqltest'";

        var result = await command.ExecuteScalarAsync();

        // Убеждаемся, что тип — Guid
        Assert.IsType<Guid>(result);
        Assert.Equal(userId.Value, (Guid)result);
    }
}

