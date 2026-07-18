using KnigoYozh.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Регистрируем наш глобальный обработчик
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Добавляем встроенную фабрику ProblemDetails (без нее IProblemDetailsService не заинжектится)
builder.Services.AddProblemDetails();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// Включаем глобальный перехват (в .NET 8 вызывается без параметров)
app.UseExceptionHandler();

// Остальные middleware...
//app.UseRouting();
//app.UseAuthentication();
////app.UseAuthorization();
//app.MapControllers();

app.Run();
