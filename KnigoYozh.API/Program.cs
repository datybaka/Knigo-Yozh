using DotNetEnv;
using KnigoYozh.Api.Infrastructure;
using KnigoYozh.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

var root = Directory.GetCurrentDirectory();
Env.Load(Path.Combine(root, ".env"));

builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

app.Run();
