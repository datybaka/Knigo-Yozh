using KnigoYozh.Application.Interfaces;
using KnigoYozh.Infrastructure.Authentication;
using KnigoYozh.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
                npgsqlOptions.MigrationsAssembly(typeof(KnigoYozhDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IKnigoYozhDbContext, KnigoYozhDbContext>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}