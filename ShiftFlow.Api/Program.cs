using ShiftFlow.Api.Dtos;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<EmployeeDto> Employees = [
  new (
    1,
    "Ali Mohammad",
    "Cleaning"
  ),
  new (
    2,
    "Mohammad Khalid",
    "Manager"
  ),
];

app.MapGet("/employee", () => Employees);

app.MapGet("/employee/{id}", (int id) => Employees.Find(Employee => Employee.Id == id))
    .WithName("GetEmployee");

app.MapPost("/employee", (CreateEmployeeDto newEmployee) =>
{
    EmployeeDto Employee = new (
        Employees.Count + 1,
        newEmployee.Name,
        newEmployee.Task
    );

    return Results.CreatedAtRoute("GetEmployee", new {id = Employee.Id}, Employee);
});

app.Run();
