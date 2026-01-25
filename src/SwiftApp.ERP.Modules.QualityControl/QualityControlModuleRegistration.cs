using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.QualityControl.Application.DTOs;
using SwiftApp.ERP.Modules.QualityControl.Application.Services;
using SwiftApp.ERP.Modules.QualityControl.Application.Validators;
using SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;
using SwiftApp.ERP.Modules.QualityControl.Infrastructure;
using SwiftApp.ERP.Modules.QualityControl.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.QualityControl;

public static class QualityControlModuleRegistration
{
    public static IServiceCollection AddQualityControlModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IInspectionPlanRepository, InspectionPlanRepository>();
        services.AddScoped<IQualityCheckRepository, QualityCheckRepository>();
        services.AddScoped<INonConformanceReportRepository, NonConformanceReportRepository>();

        // Services
        services.AddScoped<InspectionPlanService>();
        services.AddScoped<QualityCheckService>();
        services.AddScoped<NonConformanceReportService>();

        // Module API facade
        services.AddScoped<IQualityControlModuleApi, QualityControlModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<InspectionPlanRequest>, InspectionPlanRequestValidator>();
        services.AddScoped<IValidator<QualityCheckRequest>, QualityCheckRequestValidator>();
        services.AddScoped<IValidator<NcrRequest>, NcrRequestValidator>();

        return services;
    }
}
