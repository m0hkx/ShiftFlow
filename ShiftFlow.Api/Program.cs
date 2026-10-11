using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiftFlow.Api.Data;
using ShiftFlow.Api.Endpoints;
using ShiftFlow.Api.Entities;
using ShiftFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("ShiftFlow");
builder.Services.AddDbContext<ShiftFlowDbContext>(options => options.UseSqlServer(connString));

builder.Services.AddValidation();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.Manager, policy => policy.RequireRole(Roles.Manager));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapEmployeesEndpoints();
app.MapAuthEndpoints();
app.MapShiftEndpoints();
app.MapInvitationEndpoints();
app.MapMemberEndpoints();

app.Run();