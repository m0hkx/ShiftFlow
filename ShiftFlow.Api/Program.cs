using ShiftFlow.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
var app = builder.Build();

app.MapEmployeesEndpoints();

app.Run();