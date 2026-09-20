using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Andromeda.Features.Auth.IssueToken;

public record TokenRequest(string Username, string Role);
public record TokenResponse(string AccessToken, DateTime ExpiresAt);

public static class IssueTokenEndpoint
{
    public static void MapIssueToken(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", (TokenRequest request, IConfiguration configuration) =>
        {
            if (request.Role is not ("Admin" or "Viewer"))
                return Results.BadRequest(new { error = "Role must be admin or viewer" });

            var jwtSettings = configuration.GetSection("Jwt");
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]!);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, request.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return Results.Ok(new TokenResponse(accessToken, expiresAt));
        });
    }
}