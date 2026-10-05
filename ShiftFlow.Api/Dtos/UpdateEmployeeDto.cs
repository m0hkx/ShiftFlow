using System.ComponentModel.DataAnnotations;

namespace ShiftFlow.Api.Dtos;

public record UpdateEmployeeDto(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required] string Position,
    string? Team,
    bool Active
);