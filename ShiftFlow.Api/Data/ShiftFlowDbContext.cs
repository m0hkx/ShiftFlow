using Microsoft.EntityFrameworkCore;
using ShiftFlow.Api.Endpoints;
using ShiftFlow.Api.Entities;

namespace ShiftFlow.Api.Data;

public class ShiftFlowDbContext(DbContextOptions<ShiftFlowDbContext> options): DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<User>(entity =>
        {
           entity.HasIndex(u => u.Email).IsUnique(); 
           entity.HasIndex(u => u.Username).IsUnique(); 
        });
    }
}