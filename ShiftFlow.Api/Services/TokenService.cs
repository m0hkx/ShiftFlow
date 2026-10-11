using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

using ShiftFlow.Api.Entities;

namespace ShiftFlow.Api.Services;

public class TokenService(IConfiguration config)
{
    public string CreateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(CurrentUser.OrgIdClaim, user.OrgId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Creates a new access token and refresh token, and stores the refresh token's
    // hash on the user. The caller still has to call SaveChangesAsync.
    public (string AccessToken, string RefreshToken) IssueTokens(User user)
    {
        var accessToken = CreateToken(user);
        var refreshToken = CreateRefreshToken();

        user.RefreshTokenHash = HashRefreshToken(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);

        return (accessToken, refreshToken);
    }

    public string CreateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(hash);
    }
}
