using KnigoYozh.Api.Infrastructure;
using KnigoYozh.Api.Services;
using KnigoYozh.Application;
using KnigoYozh.Application.Interfaces;
using KnigoYozh.Infrastructure;
using KnigoYozh.Infrastructure.Authentication;
using KnigoYozh.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using DotNetEnv;


// 1. Загрузка .env (безопасная для Docker/CI/CD)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// 2. Регистрация сервисов слоев (Clean Architecture)
builder.Services
    .AddApplication()                        // MediatR + FluentValidation + Behaviors
    .AddInfrastructure(builder.Configuration); // DB Context + PasswordHasher

builder.Services
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails();

builder.Services
    .AddHttpContextAccessor()
    .AddScoped<ICurrentUserService, CurrentUserService>();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()!;

builder.Services
    .AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.Secret))
    };
});

// 3. Регистрация Web/API компонентов
builder.Services.AddControllers(); // или AddEndpointsApiExplorer() для Minimal API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<KnigoYozhDbContext>();
        // Если БД в Докере стартует медленнее, чем API, приложение не упадет сразу,
        // а попытается применить миграции, когда БД будет доступна (потребуются настройки retry в EF).
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Произошла ошибка при применении миграций к базе данных.");
        // Можно пробросить throw дальше, если старт без БД не имеет смысла
        throw;
    }
}

// 4. Настройка HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles(); // Заставит Kestrel искать index.html по умолчанию
app.UseStaticFiles();  // Разрешит раздачу статических файлов из wwwroot

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();