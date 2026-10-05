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

        group.MapPost("/register", async (RegisterDto registerDto, ShiftFlowDbContext db) =>
        {
            var exists = await db.Users.AnyAsync(u =>
                u.Email == registerDto.Email || u.Username == registerDto.Username);

            if (exists) return Results.Conflict("Email or username already in use.");

            var hasher = new PasswordHasher<User>();
            var newUser = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = "",
                Role = "Employee"
            };

            newUser.PasswordHash = hasher.HashPassword(newUser, registerDto.Password);

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return Results.Created($"/api/users/{newUser.Id}", new { newUser.Id, newUser.Username, newUser.Email, newUser.Role });
        });

        group.MapPost("/login", async (LoginDto loginDto, ShiftFlowDbContext db, TokenService tokenService) =>
        {
            var hasher = new PasswordHasher<User>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user is null) return Results.Unauthorized();

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (result == PasswordVerificationResult.Failed) return Results.Unauthorized();

            var token = tokenService.CreateToken(user);

            return Results.Ok(new AuthResponseDto(token, user.Username, user.Role));
        });
    }
}
