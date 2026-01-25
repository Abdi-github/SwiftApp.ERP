using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using SwiftApp.ERP.Modules.Accounting;
using SwiftApp.ERP.Modules.Auth;
using SwiftApp.ERP.WebApi.Infrastructure.SeedData;
using SwiftApp.ERP.Modules.Crm;
using SwiftApp.ERP.Modules.Hr;
using SwiftApp.ERP.Modules.Inventory;
using SwiftApp.ERP.Modules.MasterData;
using SwiftApp.ERP.Modules.Notification;
using SwiftApp.ERP.Modules.Notification.Infrastructure;
using SwiftApp.ERP.Modules.Production;
using SwiftApp.ERP.Modules.Purchasing;
using SwiftApp.ERP.Modules.QualityControl;
using SwiftApp.ERP.Modules.Sales;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;
using SwiftApp.ERP.SharedKernel.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SwiftApp ERP WebApi");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    // ── Database (PostgreSQL + EF Core) ──
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql.MigrationsAssembly("SwiftApp.ERP.WebApi")));

    // ── Redis (Distributed Caching) ──
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        options.InstanceName = "SwiftAppERP:";
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    // Register module EF configuration assemblies
    AppDbContext.ConfigurationAssemblies.Add(typeof(AuthModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(MasterDataModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(InventoryModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(SalesModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(PurchasingModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(ProductionModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(AccountingModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(HrModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(CrmModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(QualityControlModuleRegistration).Assembly);
    AppDbContext.ConfigurationAssemblies.Add(typeof(NotificationModuleRegistration).Assembly);

    // ── MediatR ──
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(AuthModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(MasterDataModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(InventoryModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(SalesModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(PurchasingModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(ProductionModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(AccountingModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(HrModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(CrmModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(QualityControlModuleRegistration).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(NotificationModuleRegistration).Assembly);
        // Register WebApi assembly for cross-module notification event handlers
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    });

    // ── Module Registrations ──
    builder.Services.AddAuthModule();
    builder.Services.AddMasterDataModule();
    builder.Services.AddInventoryModule();
    builder.Services.AddSalesModule();
    builder.Services.AddPurchasingModule();
    builder.Services.AddProductionModule();
    builder.Services.AddAccountingModule();
    builder.Services.AddHrModule();
    builder.Services.AddCrmModule();
    builder.Services.AddQualityControlModule();
    builder.Services.AddNotificationModule();

    // ── Quartz (Background Job Scheduler) ──
    builder.Services.AddQuartz(q =>
    {
        q.AddNotificationJobs();
    });
    builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

    // ── SignalR ──
    builder.Services.AddSignalR();

    // ── JWT Authentication ──
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "swiftapp-erp-api",
            ValidAudience = jwtSection["Audience"] ?? "swiftapp-erp-client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

    // ── Authorization Policies ──
    // Each policy checks for a specific permission claim OR the ADMIN role as fallback.
    string[] allPolicies =
    [
        "ADMIN:USERS_VIEW", "ADMIN:USERS_MANAGE", "ADMIN:ROLES_VIEW", "ADMIN:ROLES_MANAGE",
        "ADMIN:SETTINGS_VIEW", "ADMIN:SETTINGS_MANAGE",
        "MASTERDATA:VIEW", "MASTERDATA:CREATE", "MASTERDATA:EDIT", "MASTERDATA:DELETE",
        "INVENTORY:VIEW", "INVENTORY:CREATE", "INVENTORY:EDIT", "INVENTORY:DELETE", "INVENTORY:ADJUST",
        "SALES:VIEW", "SALES:CREATE", "SALES:EDIT", "SALES:DELETE", "SALES:APPROVE",
        "PURCHASING:VIEW", "PURCHASING:CREATE", "PURCHASING:EDIT", "PURCHASING:DELETE", "PURCHASING:APPROVE",
        "PRODUCTION:VIEW", "PRODUCTION:CREATE", "PRODUCTION:EDIT", "PRODUCTION:DELETE", "PRODUCTION:PLAN",
        "ACCOUNTING:VIEW", "ACCOUNTING:CREATE", "ACCOUNTING:EDIT", "ACCOUNTING:DELETE", "ACCOUNTING:CLOSE",
        "ACCOUNTING:APPROVE",
        "HR:VIEW", "HR:CREATE", "HR:EDIT", "HR:DELETE",
        "CRM:VIEW", "CRM:CREATE", "CRM:EDIT", "CRM:DELETE",
        "QC:VIEW", "QC:CREATE", "QC:EDIT", "QC:DELETE", "QC:APPROVE",
        "QUALITY_CONTROL:VIEW", "QUALITY_CONTROL:CREATE", "QUALITY_CONTROL:EDIT",
        "QUALITY_CONTROL:DELETE",
        "NOTIFICATION:VIEW", "NOTIFICATION:CREATE", "NOTIFICATION:MANAGE",
        "DASHBOARD:VIEW"
    ];

    var authBuilder = builder.Services.AddAuthorizationBuilder();
    foreach (var policy in allPolicies)
    {
        authBuilder.AddPolicy(policy, p => p.RequireAssertion(ctx =>
            ctx.User.HasClaim("permission", policy) || ctx.User.IsInRole("ADMIN")));
    }

    // ── Health Checks ──
    builder.Services.AddHealthChecks();

    // ── Swagger ──
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "SwiftApp ERP API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new()
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter JWT token"
        });
        c.AddSecurityRequirement(new()
        {
            {
                new()
                {
                    Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── Controllers ──
    builder.Services.AddControllers();

    // ── CORS ──
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:5001", "https://localhost:5001")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    // ── Global Exception Handler (ProblemDetails) ──
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
            var (statusCode, title) = exception switch
            {
                EntityNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business Rule Violation"),
                ConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency Conflict"),
                ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = $"https://httpstatuses.com/{statusCode}",
                title,
                status = statusCode,
                detail = exception?.Message,
                instance = context.Request.Path.Value
            });
        });
    });

    // Root redirect → Swagger
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

    // Swagger in Development
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SwiftApp ERP API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");

    // ── Auto-migrate + seed in Development ──
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAllAsync(app.Services);
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SwiftApp ERP WebApi terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory in integration tests
namespace SwiftApp.ERP.WebApi
{
    public partial class Program;
}
