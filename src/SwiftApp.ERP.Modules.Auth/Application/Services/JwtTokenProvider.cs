using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;

namespace SwiftApp.ERP.Modules.Auth.Application.Services;

/// <summary>
/// Generates JWT tokens for authenticated users.
/// Maps to Java: JwtTokenProvider in the security module.
/// </summary>
public class JwtTokenProvider(IConfiguration configuration, ILogger<JwtTokenProvider> logger)
{
    public string GenerateToken(User user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        var issuer = jwtSection["Issuer"] ?? "swiftapp-erp-api";
        var audience = jwtSection["Audience"] ?? "swiftapp-erp-client";
        var expirationMinutes = int.Parse(jwtSection["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));

            // Add permission claims from each role
            foreach (var permission in role.Permissions)
            {
                claims.Add(new Claim("permission", permission.Code));
            }
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        logger.LogInformation("JWT token generated for user {Username}", user.Username);
        return tokenString;
    }
}
