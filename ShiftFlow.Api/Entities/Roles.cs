namespace ShiftFlow.Api.Entities;

public static class Roles
{
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static bool IsValid(string role) => role is Manager or Employee;
}
