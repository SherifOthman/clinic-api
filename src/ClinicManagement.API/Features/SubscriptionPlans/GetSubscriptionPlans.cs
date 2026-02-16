using ClinicManagement.API.Common;
namespace ClinicManagement.API.Features.SubscriptionPlans;

public class GetSubscriptionPlansEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/subscription-plans", HandleAsync)
            .AllowAnonymous()
            .CacheOutput("ReferenceData")
            .WithName("GetSubscriptionPlans")
            .WithSummary("Get all subscription plans")
            .WithTags("Subscription Plans")
            .Produces<List<Response>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var plans = await db.SubscriptionPlans
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.MonthlyFee)
            .Select(sp => new Response(
                sp.Id,
                sp.Name,
                sp.Description,
                sp.MonthlyFee,
                sp.YearlyFee,
                sp.MaxBranches,
                sp.MaxStaff,
                sp.HasInventoryManagement,
                sp.HasReporting,
                sp.IsActive
            ))
            .ToListAsync(ct);

        return Results.Ok(plans);
    }

    public record Response(
        Guid Id,
        string Name,
        string Description,
        decimal MonthlyFee,
        decimal YearlyFee,
        int MaxBranches,
        int MaxStaff,
        bool HasInventoryManagement,
        bool HasReporting,
        bool IsActive);
}
