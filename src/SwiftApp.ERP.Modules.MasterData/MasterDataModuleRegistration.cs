using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Application.Services;
using SwiftApp.ERP.Modules.MasterData.Application.Validators;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.Modules.MasterData.Infrastructure;
using SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.MasterData;

public static class MasterDataModuleRegistration
{
    public static IServiceCollection AddMasterDataModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        services.AddScoped<IBillOfMaterialRepository, BillOfMaterialRepository>();

        // Services
        services.AddScoped<ProductService>();
        services.AddScoped<MaterialService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<UnitOfMeasureService>();
        services.AddScoped<BillOfMaterialService>();

        // Module API facade
        services.AddScoped<IMasterDataModuleApi, MasterDataModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<ProductRequest>, ProductRequestValidator>();
        services.AddScoped<IValidator<MaterialRequest>, MaterialRequestValidator>();
        services.AddScoped<IValidator<CategoryRequest>, CategoryRequestValidator>();
        services.AddScoped<IValidator<UnitOfMeasureRequest>, UnitOfMeasureRequestValidator>();
        services.AddScoped<IValidator<BillOfMaterialRequest>, BillOfMaterialRequestValidator>();

        return services;
    }
}
