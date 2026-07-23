using KnigoYozh.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnigoYozh.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<KnigoYozhDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Указываем сборку с миграциями (где живет AppDbContext)
                npgsqlOptions.MigrationsAssembly(typeof(KnigoYozhDbContext).Assembly.FullName);
            });
        });

        return services;
    }
}