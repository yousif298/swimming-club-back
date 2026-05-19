using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Infrastructure.Persistence;
using SwimmingClub.Infrastructure.Services;

namespace SwimmingClub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
            options.UseSqlite(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITokenService, TokenService>();
services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
