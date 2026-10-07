namespace ShiftFlow.Api.Dtos;

public record ShiftDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateTime StartTime,
    DateTime EndTime,
    string? Notes);

public record CreateShiftDto(
    Guid EmployeeId,
    DateTime StartTime,
    DateTime EndTime,
    string? Notes);

public record UpdateShiftDto(
    Guid EmployeeId,
    DateTime StartTime,
    DateTime EndTime,
    string? Notes);
