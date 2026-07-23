using DotNetEnv;
using KnigoYozh.Api.Infrastructure;
using KnigoYozh.Application;
using KnigoYozh.Infrastructure;

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
    .AddInfrastructure(builder.Configuration) // DB Context + PasswordHasher
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails();

// 3. Регистрация Web/API компонентов
builder.Services.AddControllers(); // или AddEndpointsApiExplorer() для Minimal API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Настройка HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.MapControllers(); // Маппим контроллеры

app.Run();