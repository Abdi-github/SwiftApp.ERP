using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.Inventory.Application.DTOs;
using SwiftApp.ERP.Modules.Inventory.Application.Services;
using SwiftApp.ERP.Modules.Inventory.Application.Validators;
using SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;
using SwiftApp.ERP.Modules.Inventory.Infrastructure;
using SwiftApp.ERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.Inventory;

public static class InventoryModuleRegistration
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IStockLevelRepository, StockLevelRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();

        // Services
        services.AddScoped<WarehouseService>();
        services.AddScoped<StockService>();

        // Module API facade
        services.AddScoped<IInventoryModuleApi, InventoryModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<WarehouseRequest>, WarehouseRequestValidator>();
        services.AddScoped<IValidator<StockMovementRequest>, StockMovementRequestValidator>();

        return services;
    }
}
