using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.Purchasing.Application.DTOs;
using SwiftApp.ERP.Modules.Purchasing.Application.Services;
using SwiftApp.ERP.Modules.Purchasing.Application.Validators;
using SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;
using SwiftApp.ERP.Modules.Purchasing.Infrastructure;
using SwiftApp.ERP.Modules.Purchasing.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.Purchasing;

public static class PurchasingModuleRegistration
{
    public static IServiceCollection AddPurchasingModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IPurchaseOrderLineRepository, PurchaseOrderLineRepository>();

        // Services
        services.AddScoped<SupplierService>();
        services.AddScoped<PurchaseOrderService>();

        // Module API facade
        services.AddScoped<IPurchasingModuleApi, PurchasingModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<SupplierRequest>, SupplierRequestValidator>();
        services.AddScoped<IValidator<PurchaseOrderRequest>, PurchaseOrderRequestValidator>();
        services.AddScoped<IValidator<PurchaseOrderLineRequest>, PurchaseOrderLineRequestValidator>();

        return services;
    }
}
