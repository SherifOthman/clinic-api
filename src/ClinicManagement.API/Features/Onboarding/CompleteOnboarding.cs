using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;

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
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;

        // Onboarding flow: User registers → Completes onboarding → Gets ClinicId
        // After onboarding, user can access clinic-scoped features
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.USER_NOT_FOUND,
                Title = "User Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "User not found"
            });

        if (user.ClinicId != null)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.ALREADY_ONBOARDED,
                Title = "Already Onboarded",
                Status = StatusCodes.Status400BadRequest,
                Detail = "User has already completed onboarding"
            });

        // Verify subscription plan exists
        var planExists = await db.SubscriptionPlans
            .AnyAsync(sp => sp.Id == request.SubscriptionPlanId && sp.IsActive, ct);

        if (!planExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.PLAN_NOT_FOUND,
                Title = "Plan Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Subscription plan not found or inactive"
            });

        // Create clinic
        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = request.ClinicName,
            OwnerUserId = userId,
            SubscriptionPlanId = request.SubscriptionPlanId
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
            CityGeoNameId = request.CityGeoNameId
        };

        db.ClinicBranches.Add(branch);

        // Link user to clinic (enables multi-tenancy filtering)
        user.ClinicId = clinic.Id;
        user.OnboardingCompleted = true;

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
        [MaxLength(200)]
        string ClinicName,
        
        [Required]
        Guid SubscriptionPlanId,
        
        [Required]
        [MaxLength(200)]
        string BranchName,
        
        [Required]
        [MaxLength(500)]
        string AddressLine,
        
        [Required]
        [Range(1, int.MaxValue)]
        int CountryGeoNameId,
        
        [Required]
        [Range(1, int.MaxValue)]
        int StateGeoNameId,
        
        [Required]
        [Range(1, int.MaxValue)]
        int CityGeoNameId);

    public record Response(
        Guid ClinicId,
        Guid BranchId,
        string Message);
}
