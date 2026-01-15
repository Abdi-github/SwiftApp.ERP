using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.Auth.Application.Services;
using SwiftApp.ERP.Modules.Auth.Application.Validators;
using SwiftApp.ERP.Modules.Auth.Domain.Interfaces;
using SwiftApp.ERP.Modules.Auth.Infrastructure;
using SwiftApp.ERP.Modules.Auth.Infrastructure.Persistence.Repositories;
using SwiftApp.ERP.SharedKernel.Interfaces;

namespace SwiftApp.ERP.Modules.Auth;

public static class AuthModuleRegistration
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Services
        services.AddScoped<UserService>();
        services.AddScoped<RoleService>();
        services.AddScoped<JwtTokenProvider>();

        // CurrentUserService (requires IHttpContextAccessor)
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Module API facade
        services.AddScoped<IAuthModuleApi, AuthModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<Application.DTOs.LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<Application.DTOs.UserRequest>, UserRequestValidator>();
        services.AddScoped<IValidator<Application.DTOs.RoleRequest>, RoleRequestValidator>();

        return services;
    }
}
