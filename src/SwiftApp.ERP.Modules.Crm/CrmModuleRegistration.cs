using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.Crm.Application.DTOs;
using SwiftApp.ERP.Modules.Crm.Application.Services;
using SwiftApp.ERP.Modules.Crm.Application.Validators;
using SwiftApp.ERP.Modules.Crm.Domain.Interfaces;
using SwiftApp.ERP.Modules.Crm.Infrastructure;
using SwiftApp.ERP.Modules.Crm.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.Crm;

public static class CrmModuleRegistration
{
    public static IServiceCollection AddCrmModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IInteractionRepository, InteractionRepository>();

        // Services
        services.AddScoped<ContactService>();
        services.AddScoped<InteractionService>();

        // Module API facade
        services.AddScoped<ICrmModuleApi, CrmModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<ContactRequest>, ContactRequestValidator>();
        services.AddScoped<IValidator<InteractionRequest>, InteractionRequestValidator>();

        return services;
    }
}
