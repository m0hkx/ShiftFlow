using Microsoft.EntityFrameworkCore;
using ShiftFlow.Api.Endpoints;
using ShiftFlow.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("ShiftFlow");
builder.Services.AddDbContext<ShiftFlowDbContext>(options => options.UseSqlServer(connString));

builder.Services.AddValidation();
var app = builder.Build();

app.MapEmployeesEndpoints();

app.Run();