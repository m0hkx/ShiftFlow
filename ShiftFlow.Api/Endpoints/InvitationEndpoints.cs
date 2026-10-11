using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ShiftFlow.Api.Data;
using ShiftFlow.Api.Dtos;
using ShiftFlow.Api.Entities;
using ShiftFlow.Api.Services;

namespace ShiftFlow.Api.Endpoints;

public static class InvitationEndpoints
{
    public static void MapInvitationEndpoints(this WebApplication app)
    {
        // Everything here is Manager only, except accepting an invitation
        var group = app.MapGroup("/api/invitations").RequireAuthorization(Policies.Manager);

        // List the organization's pending invitations
        group.MapGet("/", async (ShiftFlowDbContext db, CurrentUser currentUser) =>
        {
            var orgId = currentUser.OrgId;
            var now = DateTime.UtcNow;

            var invitations = await db.Invitations
                .Where(i => i.OrgId == orgId && i.AcceptedAt == null && i.ExpiresAt > now)
                .OrderBy(i => i.Email)
                .Select(i => new InvitationDto(i.Id, i.Email, i.Role, i.ExpiresAt))
                .ToListAsync();

            return Results.Ok(invitations);
        });

        // Invite someone by email. There is no email sending yet, so the token is
        // returned in the response and the manager shares it with the person.
        group.MapPost("/", async (CreateInvitationDto dto, ShiftFlowDbContext db, CurrentUser currentUser, TokenService tokenService) =>
        {
            if (!Roles.IsValid(dto.Role))
                return Results.BadRequest($"Role must be {Roles.Manager} or {Roles.Employee}.");

            // A user belongs to one organization, so an existing account can't be invited
            if (await db.Users.AnyAsync(u => u.Email == dto.Email))
                return Results.Conflict("A user with this email already exists.");

            var orgId = currentUser.OrgId;

            // Inviting the same email again replaces the earlier pending invitation
            var earlier = await db.Invitations
                .Where(i => i.OrgId == orgId && i.Email == dto.Email && i.AcceptedAt == null)
                .ToListAsync();

            db.Invitations.RemoveRange(earlier);

            // Same random token and hashing as refresh tokens: only the hash is stored
            var token = tokenService.CreateRefreshToken();

            Invitation invitation = new()
            {
                OrgId = orgId,
                Email = dto.Email,
                Role = dto.Role,
                TokenHash = tokenService.HashRefreshToken(token),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            db.Invitations.Add(invitation);

            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/invitations/{invitation.Id}",
                new CreatedInvitationDto(invitation.Id, invitation.Email, invitation.Role, invitation.ExpiresAt, token));
        });

        // Cancel a pending invitation
        group.MapDelete("/{id}", async (Guid id, ShiftFlowDbContext db, CurrentUser currentUser) =>
        {
            var orgId = currentUser.OrgId;

            var invitation = await db.Invitations
                .FirstOrDefaultAsync(i => i.Id == id && i.OrgId == orgId && i.AcceptedAt == null);

            if (invitation is null)
                return Results.NotFound();

            db.Invitations.Remove(invitation);

            await db.SaveChangesAsync();

            return Results.Ok();
        });

        // The invited person creates their account. The email and role come from
        // the invitation, so they can't be changed here.
        group.MapPost("/accept", async (AcceptInvitationDto dto, ShiftFlowDbContext db, TokenService tokenService, HttpContext ctx) =>
        {
            var hash = tokenService.HashRefreshToken(dto.Token);

            var invitation = await db.Invitations.FirstOrDefaultAsync(i => i.TokenHash == hash);

            if (invitation is null || invitation.AcceptedAt is not null || invitation.ExpiresAt <= DateTime.UtcNow)
                return Results.BadRequest("Invitation is invalid or has expired.");

            var exists = await db.Users.AnyAsync(u =>
                u.Email == invitation.Email || u.Username == dto.Username);

            if (exists) return Results.Conflict("Email or username already in use.");

            var hasher = new PasswordHasher<User>();

            var newUser = new User
            {
                OrgId = invitation.OrgId,
                Username = dto.Username,
                Email = invitation.Email,
                PasswordHash = "",
                Role = invitation.Role
            };

            newUser.PasswordHash = hasher.HashPassword(newUser, dto.Password);

            var (accessToken, refreshToken) = tokenService.IssueTokens(newUser);

            invitation.AcceptedAt = DateTime.UtcNow;

            AuthEndpoints.SetRefreshCookie(ctx, refreshToken);
            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return Results.Created($"/api/members/{newUser.Id}", new AuthResponseDto(accessToken, newUser.Username, newUser.Role));
        }).AllowAnonymous();
    }
}