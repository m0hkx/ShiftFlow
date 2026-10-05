using System.ComponentModel.DataAnnotations;

namespace ShiftFlow.Api.Dtos;

public record RegisterDto(
    [Required, StringLength(50)] string Username,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);

public record LoginDto(
    [Required] string Email,
    [Required] string Password);

public record AuthResponseDto(string Token, string Username, string Role);
