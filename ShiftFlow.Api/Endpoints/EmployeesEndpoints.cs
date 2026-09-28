using ShiftFlow.Api.Dtos;

namespace ShiftFlow.Api.Endpoints;

public static class EmployeesEndpoints
{
    private static readonly List<EmployeeDto> Employees = [
        new (
            1,
            "Ali Mohammad",
            "ali@acme.com",
            "Developer",
            "IT",
            true
        ),
        new (
            2,
            "Mohammad Khalid",
            "ali@acme.com",
            "Graphics Designer",
            "Designing",
            false
        ),
    ];

    public static void MapEmployeesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/employees");

        group.MapGet("/", () => Employees);

        group.MapGet("/{id}", (int id) =>
        {
            EmployeeDto? emp = Employees.Find(Employee => Employee.Id == id);

            if (emp is null)
            {
                return Results.NotFound();
            }
            else
            {
                return Results.Ok(emp);
            }
        }).WithName("GetEmployee");

        group.MapPost("/", (CreateEmployeeDto newEmployee) =>
        {
            EmployeeDto Employee = new(
                Employees.Count + 1,
                newEmployee.Name,
                newEmployee.Email,
                newEmployee.Position,
                newEmployee.Team,
                newEmployee.Active
            );

            Employees.Add(Employee);

            return Results.CreatedAtRoute("GetEmployee", new { id = Employee.Id }, Employee);
        });
    }
}