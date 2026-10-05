namespace ShiftFlow.Api.Dtos;

public record EmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string Position,
    string? Team,
    bool Active
);