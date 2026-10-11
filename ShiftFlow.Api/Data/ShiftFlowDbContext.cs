using Microsoft.EntityFrameworkCore;
using ShiftFlow.Api.Entities;
using ShiftFlow.Api.Services;

namespace ShiftFlow.Api.Data;

public class ShiftFlowDbContext(DbContextOptions<ShiftFlowDbContext> options, CurrentUser currentUser): DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    // Used by the query filters below, so every Employee and Shift query only
    // sees rows from the caller's organization
    private Guid CurrentOrgId => currentUser.OrgId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            // Two organizations can each have an employee with the same email
            entity.HasIndex(e => new { e.OrgId, e.Email }).IsUnique();

            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(e => e.OrgId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => e.OrgId == CurrentOrgId);
        });

        modelBuilder.Entity<User>(entity =>
        {
           // Users stay globally unique because login is by email
           entity.HasIndex(u => u.Email).IsUnique();
           entity.HasIndex(u => u.Username).IsUnique();

           entity.HasOne<Organization>()
               .WithMany()
               .HasForeignKey(u => u.OrgId)
               .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(s => s.OrgId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(s => s.OrgId == CurrentOrgId);
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasIndex(i => i.TokenHash).IsUnique();
            entity.Property(i => i.TokenHash).HasMaxLength(44);

            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(i => i.OrgId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
