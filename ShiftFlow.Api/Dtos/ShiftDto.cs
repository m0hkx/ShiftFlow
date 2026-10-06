using System.ComponentModel.DataAnnotations;

namespace ShiftFlow.Api.Dtos;

public record ShiftDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateTime StartTime,
    DateTime EndTime,
    string? Notes);

public record CreateShiftDto(
    [Required] Guid? EmployeeId,
    [Required] DateTime? StartTime,
    [Required] DateTime? EndTime,
    string? Notes);

public record UpdateShiftDto(
    [Required] Guid? EmployeeId,
    [Required] DateTime? StartTime,
    [Required] DateTime? EndTime,
    string? Notes);
