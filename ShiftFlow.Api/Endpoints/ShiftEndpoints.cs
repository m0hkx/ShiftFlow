using Microsoft.EntityFrameworkCore;
using ShiftFlow.Api.Data;
using ShiftFlow.Api.Dtos;
using ShiftFlow.Api.Entities;

namespace ShiftFlow.Api.Endpoints;

public static class ShiftEndpoints
{
    public static void MapShiftEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/shifts").RequireAuthorization();

        // Get Shifts (optional filters: employeeId, and shifts that start between from and to)
        group.MapGet("/", async (ShiftFlowDbContext db, Guid? employeeId, DateTime? from, DateTime? to) =>
        {
            var query = db.Shifts.AsQueryable();

            if (employeeId is not null)
                query = query.Where(s => s.EmployeeId == employeeId);

            if (from is not null)
                query = query.Where(s => s.StartTime >= from);

            if (to is not null)
                query = query.Where(s => s.StartTime < to);

            var shifts = await query
                .OrderBy(s => s.StartTime)
                .Select(s => new ShiftDto(s.Id, s.EmployeeId, s.Employee.Name, s.StartTime, s.EndTime, s.Notes))
                .ToListAsync();

            return Results.Ok(shifts);
        });

        // Get specific shift
        group.MapGet("/{id}", async (Guid id, ShiftFlowDbContext db) =>
        {
            var shiftDto = await db.Shifts
                .Where(s => s.Id == id)
                .Select(s => new ShiftDto(s.Id, s.EmployeeId, s.Employee.Name, s.StartTime, s.EndTime, s.Notes))
                .FirstOrDefaultAsync();

            if (shiftDto is null)
                return Results.NotFound();

            return Results.Ok(shiftDto);
        }).WithName("GetShift");

        // Create Shift
        group.MapPost("/", async (CreateShiftDto createShiftDto, ShiftFlowDbContext db) =>
        {
            var error = ValidateShift(createShiftDto.EmployeeId, createShiftDto.StartTime, createShiftDto.EndTime);

            if (error is not null)
                return Results.BadRequest(error);

            var employee = await db.Employees.FindAsync(createShiftDto.EmployeeId);

            if (employee is null)
                return Results.BadRequest("Employee not found.");

            if (await HasOverlap(db, createShiftDto.EmployeeId, createShiftDto.StartTime, createShiftDto.EndTime))
                return Results.Conflict("The employee already has a shift that overlaps this time.");

            Shift shiftEntity = new()
            {
                EmployeeId = createShiftDto.EmployeeId,
                StartTime = createShiftDto.StartTime,
                EndTime = createShiftDto.EndTime,
                Notes = createShiftDto.Notes,
            };

            db.Shifts.Add(shiftEntity);

            await db.SaveChangesAsync();

            ShiftDto shiftDto = new(
                shiftEntity.Id,
                shiftEntity.EmployeeId,
                employee.Name,
                shiftEntity.StartTime,
                shiftEntity.EndTime,
                shiftEntity.Notes);

            return Results.CreatedAtRoute("GetShift", new { id = shiftEntity.Id }, shiftDto);
        });

        // Edit Shift
        group.MapPut("/{id}", async (Guid id, UpdateShiftDto updateShiftDto, ShiftFlowDbContext db) =>
        {
            var shift = await db.Shifts.FindAsync(id);

            if (shift is null)
                return Results.NotFound();

            var error = ValidateShift(updateShiftDto.EmployeeId, updateShiftDto.StartTime, updateShiftDto.EndTime);

            if (error is not null)
                return Results.BadRequest(error);

            if (await db.Employees.FindAsync(updateShiftDto.EmployeeId) is null)
                return Results.BadRequest("Employee not found.");

            if (await HasOverlap(db, updateShiftDto.EmployeeId, updateShiftDto.StartTime, updateShiftDto.EndTime, id))
                return Results.Conflict("The employee already has a shift that overlaps this time.");

            shift.EmployeeId = updateShiftDto.EmployeeId;
            shift.StartTime = updateShiftDto.StartTime;
            shift.EndTime = updateShiftDto.EndTime;
            shift.Notes = updateShiftDto.Notes;

            await db.SaveChangesAsync();

            return Results.Ok();
        });

        // Delete Shift
        group.MapDelete("/{id}", async (Guid id, ShiftFlowDbContext db) =>
        {
            var shift = await db.Shifts.FindAsync(id);

            if (shift is null)
                return Results.NotFound();

            db.Shifts.Remove(shift);

            await db.SaveChangesAsync();

            return Results.Ok();
        });
    }

    // Returns an error message, or null when the shift data is valid
    private static string? ValidateShift(Guid employeeId, DateTime startTime, DateTime endTime)
    {
        if (employeeId == Guid.Empty)
            return "EmployeeId is required.";

        if (startTime == default || endTime == default)
            return "StartTime and EndTime are required.";

        if (endTime <= startTime)
            return "EndTime must be after StartTime.";

        return null;
    }

    // True when the employee already has a shift that overlaps startTime-endTime.
    private static Task<bool> HasOverlap(
        ShiftFlowDbContext db, Guid employeeId, DateTime startTime, DateTime endTime, Guid? excludeShiftId = null)
    {
        return db.Shifts.AnyAsync(s =>
            s.EmployeeId == employeeId
            && (excludeShiftId == null || s.Id != excludeShiftId)
            && s.StartTime < endTime
            && s.EndTime > startTime);
    }
}
