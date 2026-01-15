using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SwiftApp.ERP.SharedKernel.Interfaces;

namespace SwiftApp.ERP.Modules.Auth.Application.Services;

/// <summary>
/// Reads the current user's identity from the HttpContext claims.
/// Implements ICurrentUserService from SharedKernel.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Username
        => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue("unique_name");

    public bool IsAuthenticated
        => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles
        => httpContextAccessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly()
        ?? (IReadOnlyList<string>)Array.Empty<string>();

    public bool IsInRole(string role)
        => httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
}
