using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using SwiftApp.ERP.Modules.Accounting;
using SwiftApp.ERP.Modules.Auth;
using SwiftApp.ERP.Modules.Auth.Application.Services;
using SwiftApp.ERP.Modules.Crm;
using SwiftApp.ERP.Modules.Hr;
using SwiftApp.ERP.Modules.Inventory;
using SwiftApp.ERP.Modules.MasterData;
using SwiftApp.ERP.Modules.Notification;
using SwiftApp.ERP.Modules.Production;
using SwiftApp.ERP.Modules.Purchasing;
using SwiftApp.ERP.Modules.QualityControl;
using SwiftApp.ERP.Modules.Sales;
using SwiftApp.ERP.SharedKernel.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;
using SwiftApp.ERP.SharedKernel.Services;
using Microsoft.AspNetCore.Authentication;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SwiftApp ERP WebApp");

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

    // ── Cookie Authentication for Blazor SSR ──
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

    // ── Authorization Policies (same as WebApi) ──
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

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddHttpContextAccessor();

    // ── Localization ──
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    // ── Blazor Server-Side Rendering ──
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // ── SignalR for real-time notifications ──
    builder.Services.AddSignalR();

    // ── Health checks ──
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseStaticFiles();
    app.UseAntiforgery();
    app.UseSerilogRequestLogging();

    // ── Request localization: de-CH (default), fr-CH, it-CH, en ──
    var supportedCultures = new[] { "de-CH", "fr-CH", "it-CH", "en" };
    app.UseRequestLocalization(options =>
    {
        options.SetDefaultCulture("de-CH");
        options.AddSupportedCultures(supportedCultures);
        options.AddSupportedUICultures(supportedCultures);
        options.RequestCultureProviders.Insert(0,
            new Microsoft.AspNetCore.Localization.CookieRequestCultureProvider { CookieName = "erp-locale" });
    });

    app.UseAuthentication();
    app.UseAuthorization();

    // Root redirect → /app/dashboard (which requires auth → redirects to /login)
    app.MapGet("/", () => Results.Redirect("/app/dashboard"));
    // Redirect /app to /app/dashboard
    app.MapGet("/app", () => Results.Redirect("/app/dashboard"));

    // ── Login endpoint (handles form POST from Blazor SSR login page) ──
    app.MapPost("/account/login", async (HttpContext httpContext, UserService userService) =>
    {
        var form = await httpContext.Request.ReadFormAsync();
        var username = form["username"].ToString();
        var password = form["password"].ToString();
        var returnUrl = form["returnUrl"].ToString();

        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith("/"))
            returnUrl = "/app/dashboard";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Results.Redirect($"/login?error=required&returnUrl={Uri.EscapeDataString(returnUrl)}");

        try
        {
            var user = await userService.AuthenticateAsync(username, password);
            if (user is null)
                return Results.Redirect($"/login?error=invalid&returnUrl={Uri.EscapeDataString(returnUrl)}");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName)
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
                foreach (var permission in role.Permissions)
                {
                    claims.Add(new Claim("permission", permission.Code));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            return Results.Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Login failed for user {Username}", username);
            return Results.Redirect($"/login?error=server&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
    }).AllowAnonymous();

    // ── Logout endpoint ──
    app.MapGet("/account/logout", async (HttpContext httpContext) =>
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/login");
    }).AllowAnonymous();

    app.MapHealthChecks("/health");

    // ── SignalR Hub ──
    app.MapHub<SwiftApp.ERP.Modules.Notification.Infrastructure.NotificationHub>("/hubs/notifications");

    app.MapRazorComponents<SwiftApp.ERP.WebApp.Components.App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SwiftApp ERP WebApp terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace SwiftApp.ERP.WebApp
{
    public partial class Program;
}
