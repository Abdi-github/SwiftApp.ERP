using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.Accounting.Application.DTOs;
using SwiftApp.ERP.Modules.Accounting.Application.Services;
using SwiftApp.ERP.Modules.Accounting.Application.Validators;
using SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;
using SwiftApp.ERP.Modules.Accounting.Infrastructure;
using SwiftApp.ERP.Modules.Accounting.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.Accounting;

public static class AccountingModuleRegistration
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

        // Services
        services.AddScoped<AccountService>();
        services.AddScoped<JournalEntryService>();

        // Module API facade
        services.AddScoped<IAccountingModuleApi, AccountingModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<AccountRequest>, AccountRequestValidator>();
        services.AddScoped<IValidator<JournalEntryRequest>, JournalEntryRequestValidator>();
        services.AddScoped<IValidator<JournalEntryLineRequest>, JournalEntryLineRequestValidator>();

        return services;
    }
}
