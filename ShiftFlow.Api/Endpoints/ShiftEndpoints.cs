namespace ShiftFlow.Api.Endpoints;

public static class ShiftEndpoints
{
    public static void MapShiftEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/shifts").RequireAuthorization();

        // Get Shifts
        group.MapGet("/", async () =>
        {
            
        });

        // Create Shift
        group.MapPost("/", async () =>
        {
            
        });

        // Edit Shift
        group.MapPut("/{id}", async () =>
        {
            
        });


        // Delete Shift
        group.MapDelete("/{id}", async () =>
        {
            
        });
    }
} 