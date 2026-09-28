namespace ShiftFlow.Api.Dtos;

public record EmployeeDto(
    int Id,
    string Name,
    string Email,
    string Position,
    string Team,
    bool Active
);