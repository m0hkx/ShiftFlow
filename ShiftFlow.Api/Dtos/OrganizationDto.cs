using System.ComponentModel.DataAnnotations;

namespace ShiftFlow.Api.Dtos;

public record CreateInvitationDto(
    [Required, EmailAddress] string Email,
    [Required] string Role);

public record CreatedInvitationDto(Guid Id, string Email, string Role, DateTime ExpiresAt, string Token);

public record InvitationDto(Guid Id, string Email, string Role, DateTime ExpiresAt);

public record AcceptInvitationDto(
    [Required] string Token,
    [Required, StringLength(50)] string Username,
    [Required, MinLength(8)] string Password);

public record MemberDto(Guid Id, string Username, string Email, string Role);

public record ChangeRoleDto([Required] string Role);
