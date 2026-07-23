using FluentValidation;
using KnigoYozh.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KnigoYozh.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // 1. Регистрируем MediatR и наш ValidationBehavior
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Автоматически оборачивает ВСЕ команды в валидатор
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // 2. Сканируем сборку Application и регистрируем все валидаторы FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}