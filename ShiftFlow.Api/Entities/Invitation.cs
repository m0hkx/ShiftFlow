namespace ShiftFlow.Api.Entities;

public class Invitation
{
    public Guid Id { get; set; }
    public required Guid OrgId { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public required string TokenHash { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}
