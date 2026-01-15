namespace SwiftApp.ERP.SharedKernel.Interfaces;

/// <summary>
/// Provides information about the currently authenticated user.
/// Implemented in the host project (WebApi / WebApp).
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
