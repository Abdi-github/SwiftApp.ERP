using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SwiftApp.ERP.SharedKernel.Interfaces;

/// <summary>
/// Each module implements this to register its services, repositories,
/// validators, and EF configurations with the DI container.
/// </summary>
public interface IModuleInstaller
{
    void Install(IServiceCollection services, IConfiguration configuration);
}
