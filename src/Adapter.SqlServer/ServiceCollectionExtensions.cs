using Adapter.SqlServer.Authentication;
using Adapter.SqlServer.Data;
using Adapter.SqlServer.Repositories;
using Core.Contracts.Authentication;
using Core.Contracts.Repositories;
using Core.Contracts.UnitOfWork;
using Core.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Adapter.SqlServer;
public static class ServiceCollectionExtensions
{
    public static void AddAdapterSqlServer(this IServiceCollection services, string connectionString)
    {
        services.ConfigureDbContext(connectionString);
        services.ConfigureHealthChecks();
        services.AddAuthenticationInternal();
        services.AddUnitOfWork();
        services.AddRepositories();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }

    private static void ConfigureDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }

    private static void ConfigureHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("Database Health", HealthStatus.Unhealthy);
    }

    private static void AddAuthenticationInternal(this IServiceCollection services)
    {
        services.AddScoped<ITokenProvider, TokenProvider>()
                .AddScoped<IUserInfo, UserInfo>()
                .AddScoped<IPasswordHasher<User>, PasswordHasher<User>>() ;
    }

    private static void AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IProjectRepository, ProjectRepository>()
                .AddScoped<IProjectUserRepository, ProjectUserRepository>()
                .AddScoped<IProjectUserRoleRepository, ProjectUserRoleRepository>()
                .AddScoped<IAssignmentRepository, AssignmentRepository>()
                .AddScoped<IAssignmentUserRepository, AssignmentUserRepository>();
    }
}