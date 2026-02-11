using ClinicManagement.API.Common;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Onboarding;

public class CompleteOnboardingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/onboarding/complete", HandleAsync)
            .RequireAuthorization()
            .WithName("CompleteOnboarding")
            .WithSummary("Complete clinic onboarding")
            .WithTags("Onboarding")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;

        // Check if user already has a clinic
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
            return Results.BadRequest(new
            {
                error = "User not found",
                code = "USER_NOT_FOUND"
            });

        if (user.ClinicId != null)
            return Results.BadRequest(new
            {
                error = "User has already completed onboarding",
                code = "ALREADY_ONBOARDED"
            });

        // Verify subscription plan exists
        var planExists = await db.SubscriptionPlans
            .AnyAsync(sp => sp.Id == request.SubscriptionPlanId && sp.IsActive, ct);

        if (!planExists)
            return Results.BadRequest(new
            {
                error = "Subscription plan not found or inactive",
                code = "PLAN_NOT_FOUND"
            });

        // Create clinic
        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = request.ClinicName,
            OwnerUserId = userId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            CreatedAt = DateTime.UtcNow
        };

        db.Clinics.Add(clinic);

        // Create branch
        var branch = new ClinicBranch
        {
            Id = Guid.NewGuid(),
            ClinicId = clinic.Id,
            Name = request.BranchName,
            AddressLine = request.AddressLine,
            CountryGeoNameId = request.CountryGeoNameId,
            StateGeoNameId = request.StateGeoNameId,
            CityGeoNameId = request.CityGeoNameId,
            CreatedAt = DateTime.UtcNow
        };

        db.ClinicBranches.Add(branch);

        // Update user with clinic
        user.ClinicId = clinic.Id;

        await db.SaveChangesAsync(ct);

        var response = new Response(
            clinic.Id,
            branch.Id,
            "Onboarding completed successfully"
        );

        return Results.Created($"/api/clinics/{clinic.Id}", response);
    }

    public record Request(
        [Required]
        [MaxLength(200, ErrorMessage = "Clinic name must not exceed 200 characters")]
        string ClinicName,
        
        [Required]
        Guid SubscriptionPlanId,
        
        [Required]
        [MaxLength(200, ErrorMessage = "Branch name must not exceed 200 characters")]
        string BranchName,
        
        [Required]
        [MaxLength(500, ErrorMessage = "Address must not exceed 500 characters")]
        string AddressLine,
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Country is required")]
        int CountryGeoNameId,
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "State is required")]
        int StateGeoNameId,
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "City is required")]
        int CityGeoNameId);

    public record Response(
        Guid ClinicId,
        Guid BranchId,
        string Message);
}
