using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ShiftFlow.Api.Data;
using ShiftFlow.Api.Dtos;
using ShiftFlow.Api.Entities;
using ShiftFlow.Api.Services;

namespace ShiftFlow.Api.Endpoints;

public static class AuthEndpoints
{
    internal static void SetRefreshCookie(HttpContext ctx, string refreshToken) =>
        ctx.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = ctx.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        // Creates a new organization with the registering user as its Manager.
        // Other people join an organization through an invitation instead.
        group.MapPost("/register", async (RegisterDto registerDto, ShiftFlowDbContext db, TokenService tokenService, HttpContext ctx) =>
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

            SetRefreshCookie(ctx, refreshToken);
            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return Results.Created($"/api/members/{newUser.Id}", new AuthResponseDto(accessToken, newUser.Username, newUser.Role));
        });

        group.MapPost("/login", async (LoginDto loginDto, ShiftFlowDbContext db, TokenService tokenService, HttpContext ctx) =>
        {
            var hasher = new PasswordHasher<User>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user is null) return Results.Unauthorized();

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (result == PasswordVerificationResult.Failed) return Results.Unauthorized();

            var (token, refreshToken) = tokenService.IssueTokens(user);

            SetRefreshCookie(ctx, refreshToken);
            await db.SaveChangesAsync();

            return Results.Ok(new AuthResponseDto(token, user.Username, user.Role));
        });

        group.MapPost("/refresh", async (HttpContext ctx, ShiftFlowDbContext db, TokenService tokenService) =>
        {
            var refreshToken = ctx.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Results.Unauthorized();

            var hash = tokenService.HashRefreshToken(refreshToken);

            var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hash);

            if (user is null)
                return Results.Unauthorized();

            if (user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return Results.Unauthorized();

            var (accessToken, newRefreshToken) = tokenService.IssueTokens(user);

            SetRefreshCookie(ctx, newRefreshToken);
            await db.SaveChangesAsync();

            return Results.Ok(new AuthResponseDto(
                accessToken,
                user.Username,
                user.Role));
        });

        group.MapPost("/logout", async (HttpContext ctx, ShiftFlowDbContext db, TokenService tokenService) =>
        {
            var refreshToken = ctx.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Results.Ok();

            var hash = tokenService.HashRefreshToken(refreshToken);
            var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hash);

            ctx.Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });

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
