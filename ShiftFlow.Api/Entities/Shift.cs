namespace ShiftFlow.Api.Entities;

public class Shift
{
    public Guid Id { get; set; }
    public required Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public string? Notes { get; set; }

}