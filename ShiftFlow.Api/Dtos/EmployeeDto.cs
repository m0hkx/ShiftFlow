using System.ComponentModel.DataAnnotations;

namespace ShiftFlow.Api.Dtos;

public record EmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string Position,
    string? Team,
    bool Active
);

public record CreateEmployeeDto(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required] string Position,
    string? Team,
    bool Active
);

public record UpdateEmployeeDto(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required] string Position,
    string? Team,
    bool Active
);
