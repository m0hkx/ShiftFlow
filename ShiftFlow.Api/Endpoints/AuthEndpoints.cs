using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ShiftFlow.Api.Data;
using ShiftFlow.Api.Dtos;
using ShiftFlow.Api.Entities;
using ShiftFlow.Api.Services;

namespace ShiftFlow.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        // Creates a new organization with the registering user as its Manager.
        // Other people join an organization through an invitation instead.
        group.MapPost("/register", async (RegisterDto registerDto, ShiftFlowDbContext db, TokenService tokenService) =>
        {
            var exists = await db.Users.AnyAsync(u =>
                u.Email == registerDto.Email || u.Username == registerDto.Username);

            if (exists) return Results.Conflict("Email or username already in use.");

            Organization organization = new() { Name = registerDto.OrganizationName };

            // Adding it generates organization.Id, which the user needs below
            db.Organizations.Add(organization);

            var hasher = new PasswordHasher<User>();

            var newUser = new User
            {
                OrgId = organization.Id,
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = "",
                Role = Roles.Manager
            };

            newUser.PasswordHash = hasher.HashPassword(newUser, registerDto.Password);

            var (accessToken, refreshToken) = tokenService.IssueTokens(newUser);

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return Results.Created($"/api/members/{newUser.Id}", new AuthResponseDto(accessToken, refreshToken, newUser.Username, newUser.Role));
        });

        group.MapPost("/login", async (LoginDto loginDto, ShiftFlowDbContext db, TokenService tokenService) =>
        {
            var hasher = new PasswordHasher<User>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user is null) return Results.Unauthorized();

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (result == PasswordVerificationResult.Failed) return Results.Unauthorized();

            var (token, refreshToken) = tokenService.IssueTokens(user);

            await db.SaveChangesAsync();

            return Results.Ok(new AuthResponseDto(token, refreshToken, user.Username, user.Role));
        });

        group.MapPost("/refresh", async (RefreshTokenDto refreshTokenDto, ShiftFlowDbContext db, TokenService tokenService) =>
        {
            var hash = tokenService.HashRefreshToken(refreshTokenDto.RefreshToken);

            var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hash);

            if (user is null)
                return Results.Unauthorized();

            if (user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return Results.Unauthorized();

            var (accessToken, newRefreshToken) = tokenService.IssueTokens(user);

            await db.SaveChangesAsync();

            return Results.Ok(new AuthResponseDto(
                accessToken,
                newRefreshToken,
                user.Username,
                user.Role));
        });

        group.MapPost("/logout", async (ShiftFlowDbContext db, RefreshTokenDto refreshTokenDto, TokenService tokenService) =>
        {
            var hash = tokenService.HashRefreshToken(refreshTokenDto.RefreshToken);
            var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hash);

            if (user is not null)
            {
                user.RefreshTokenHash = null;
                user.RefreshTokenExpiresAt = null;

                await db.SaveChangesAsync();
            }

            return Results.Ok();
        });
    }
}
