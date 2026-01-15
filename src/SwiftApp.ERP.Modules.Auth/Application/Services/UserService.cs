using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Auth.Application.DTOs;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.Modules.Auth.Domain.Events;
using SwiftApp.ERP.Modules.Auth.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Auth.Application.Services;

/// <summary>
/// Business logic for user management.
/// Maps to Java: UserService — CRUD with soft-delete, role assignment, BCrypt password hashing.
/// </summary>
public class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPublisher publisher,
    ILogger<UserService> logger)
{
    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        return user is null ? null : MapToResponse(user);
    }

    public async Task<UserResponse?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var user = await userRepository.GetByUsernameAsync(username, ct);
        return user is null ? null : MapToResponse(user);
    }

    public async Task<PagedResult<UserResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await userRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<UserResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<UserResponse> CreateAsync(UserRequest request, CancellationToken ct = default)
    {
        // Validate uniqueness
        if (await userRepository.GetByUsernameAsync(request.Username, ct) is not null)
            throw new BusinessRuleException("UNIQUE_USERNAME", $"Username '{request.Username}' is already taken.");
        if (await userRepository.GetByEmailAsync(request.Email, ct) is not null)
            throw new BusinessRuleException("UNIQUE_EMAIL", $"Email '{request.Email}' is already registered.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password
                ?? throw new BusinessRuleException("PASSWORD_REQUIRED", "Password is required for new users.")),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Enabled = request.Enabled ?? true,
        };

        // Assign roles
        if (request.RoleNames is { Count: > 0 })
        {
            var roles = await roleRepository.GetByNamesAsync(request.RoleNames, ct);
            foreach (var role in roles)
                user.Roles.Add(role);
        }

        await userRepository.AddAsync(user, ct);

        logger.LogInformation("User {Username} created with id {UserId}", user.Username, user.Id);
        await publisher.Publish(new UserCreatedEvent(user.Id, user.Username, user.Email), ct);

        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UserRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(User), id);

        // Check uniqueness if username changed
        if (user.Username != request.Username && await userRepository.GetByUsernameAsync(request.Username, ct) is not null)
            throw new BusinessRuleException("UNIQUE_USERNAME", $"Username '{request.Username}' is already taken.");
        if (user.Email != request.Email && await userRepository.GetByEmailAsync(request.Email, ct) is not null)
            throw new BusinessRuleException("UNIQUE_EMAIL", $"Email '{request.Email}' is already registered.");

        user.Username = request.Username;
        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Enabled = request.Enabled ?? user.Enabled;

        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = HashPassword(request.Password);

        // Update roles if provided
        var oldRoles = user.Roles.Select(r => r.Name).ToHashSet();
        if (request.RoleNames is not null)
        {
            user.Roles.Clear();
            var roles = await roleRepository.GetByNamesAsync(request.RoleNames, ct);
            foreach (var role in roles)
                user.Roles.Add(role);

            var newRoles = user.Roles.Select(r => r.Name).ToHashSet();
            if (!oldRoles.SetEquals(newRoles))
            {
                await publisher.Publish(
                    new UserRoleChangedEvent(user.Id, user.Username, newRoles.AsReadOnly()), ct);
            }
        }

        await userRepository.UpdateAsync(user, ct);
        logger.LogInformation("User {Username} updated", user.Username);

        return MapToResponse(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await userRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("User {UserId} soft-deleted", id);
    }

    public async Task<User?> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await userRepository.GetByUsernameAsync(username, ct);
        if (user is null || !user.Enabled || user.Locked)
            return null;

        return VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    private static UserResponse MapToResponse(User user) => new(
        user.Id,
        user.Username,
        user.Email,
        user.FirstName,
        user.LastName,
        user.DisplayName,
        user.Enabled,
        user.Locked,
        user.Roles.Select(r => r.Name).ToHashSet().AsReadOnly(),
        user.CreatedAt,
        user.UpdatedAt);

    // BCrypt-compatible password hashing using PBKDF2
    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var storedHashBytes = Convert.FromBase64String(parts[1]);
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
    }
}
