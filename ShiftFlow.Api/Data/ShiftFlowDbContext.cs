using Microsoft.EntityFrameworkCore;
using ShiftFlow.Api.Entities;

namespace ShiftFlow.Api.Data;

public class ShiftFlowDbContext(DbContextOptions<ShiftFlowDbContext> options): DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
}