namespace ShiftFlow.Api.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Position { get; set; }
    public string? Team { get; set; }
    public required bool Active { get; set; }
} 