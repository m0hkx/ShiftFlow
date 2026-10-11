using Microsoft.EntityFrameworkCore;

using ShiftFlow.Api.Data;
using ShiftFlow.Api.Dtos;
using ShiftFlow.Api.Entities;
using ShiftFlow.Api.Services;

namespace ShiftFlow.Api.Endpoints;

// Members are the user accounts in the caller's organization
public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/members").RequireAuthorization();

        group.MapGet("/", async (ShiftFlowDbContext db, CurrentUser currentUser) =>
        {
            var orgId = currentUser.OrgId;

            var members = await db.Users
                .Where(u => u.OrgId == orgId)
                .OrderBy(u => u.Username)
                .Select(u => new MemberDto(u.Id, u.Username, u.Email, u.Role))
                .ToListAsync();

            return Results.Ok(members);
        });

        // The member's access token keeps the old role until it expires or is refreshed
        group.MapPut("/{id}/role", async (Guid id, ChangeRoleDto dto, ShiftFlowDbContext db, CurrentUser currentUser) =>
        {
            if (!Roles.IsValid(dto.Role))
                return Results.BadRequest($"Role must be {Roles.Manager} or {Roles.Employee}.");

            var orgId = currentUser.OrgId;

            var member = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrgId == orgId);

            if (member is null)
                return Results.NotFound();

            if (member.Role == Roles.Manager && dto.Role != Roles.Manager && await IsLastManager(db, orgId))
                return Results.Conflict("The organization must keep at least one Manager.");

            member.Role = dto.Role;

            await db.SaveChangesAsync();

            return Results.Ok(new MemberDto(member.Id, member.Username, member.Email, member.Role));
        }).RequireAuthorization(Policies.Manager);

        group.MapDelete("/{id}", async (Guid id, ShiftFlowDbContext db, CurrentUser currentUser) =>
        {
            var orgId = currentUser.OrgId;

            var member = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrgId == orgId);

            if (member is null)
                return Results.NotFound();

            if (member.Role == Roles.Manager && await IsLastManager(db, orgId))
                return Results.Conflict("The organization must keep at least one Manager.");

            db.Users.Remove(member);

            await db.SaveChangesAsync();

            return Results.Ok();
        }).RequireAuthorization(Policies.Manager);
    }

    private static async Task<bool> IsLastManager(ShiftFlowDbContext db, Guid orgId)
    {
        var managers = await db.Users.CountAsync(u => u.OrgId == orgId && u.Role == Roles.Manager);

        return managers <= 1;
    }
}
