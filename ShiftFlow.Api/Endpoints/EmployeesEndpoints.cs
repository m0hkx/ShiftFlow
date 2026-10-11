using Microsoft.EntityFrameworkCore;
using ShiftFlow.Api.Dtos;
using ShiftFlow.Api.Data;
using ShiftFlow.Api.Entities;
using ShiftFlow.Api.Services;

namespace ShiftFlow.Api.Endpoints;

public static class EmployeesEndpoints
{

    public static void MapEmployeesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/employees").RequireAuthorization();

        group.MapGet("/", async (ShiftFlowDbContext db) =>
        {
            var employees = await db.Employees
                .Select(e => new EmployeeDto(e.Id, e.Name, e.Email, e.Position, e.Team, e.Active))
                .ToListAsync();

            return Results.Ok(employees);
        });

        group.MapGet("/{id}", async (Guid id, ShiftFlowDbContext db) =>
        {
            var emp = await db.Employees.FindAsync(id);

            if (emp is null)
            {
                return Results.NotFound();
            }
            else
            {
                EmployeeDto employeeDto = new(
                    emp.Id,
                    emp.Name,
                    emp.Email,
                    emp.Position,
                    emp.Team,
                    emp.Active
                );

                return Results.Ok(employeeDto);
            }
        }).WithName("GetEmployee");

        group.MapPost("/", async (CreateEmployeeDto newEmployee, ShiftFlowDbContext db, CurrentUser currentUser) =>
        {
            Employee emp = new()
            {
                OrgId = currentUser.OrgId,
                Name = newEmployee.Name,
                Email = newEmployee.Email,
                Position = newEmployee.Position,
                Team = newEmployee.Team,
                Active = newEmployee.Active
            };

            db.Employees.Add(emp);

            await db.SaveChangesAsync();

            EmployeeDto employeeDto = new(
                emp.Id,
                emp.Name,
                emp.Email,
                emp.Position,
                emp.Team,
                emp.Active
            );

            return Results.CreatedAtRoute("GetEmployee", new { id = emp.Id }, employeeDto);
        }).RequireAuthorization(Policies.Manager);

        group.MapPut("/{id}", async (Guid id, ShiftFlowDbContext db, UpdateEmployeeDto upEmp) =>
        {
            var employee = await db.Employees.FindAsync(id);

            if (employee is null)
                return Results.NotFound();

            employee.Name = upEmp.Name;
            employee.Email = upEmp.Email;
            employee.Position = upEmp.Position;
            employee.Team = upEmp.Team;
            employee.Active = upEmp.Active;

            await db.SaveChangesAsync();

            return Results.Ok();
        }).RequireAuthorization(Policies.Manager);

        group.MapDelete("/{id}", async (Guid id, ShiftFlowDbContext db) =>
        {
            var employee = await db.Employees.FindAsync(id);

            if (employee is null)
                return Results.NotFound();

            db.Employees.Remove(employee);

            await db.SaveChangesAsync();

            return Results.Ok();
        }).RequireAuthorization(Policies.Manager);
    }
}