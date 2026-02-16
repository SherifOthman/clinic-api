using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;

namespace ClinicManagement.API.Features.Onboarding;

public class CompleteOnboardingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/onboarding/complete", HandleAsync)
            .RequireAuthorization("ClinicManagement")
            .WithName("CompleteOnboarding")
            .WithSummary("Complete clinic onboarding")
            .WithTags("Onboarding")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        DateTimeProvider dateTimeProvider,
        CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;

        // Onboarding flow: User registers → Completes onboarding → Creates Clinic + Gets Staff record
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

        // Check if user already has a clinic (already onboarded)
        var existingClinic = await db.Clinics
            .FirstOrDefaultAsync(c => c.OwnerUserId == userId, ct);
            
        if (existingClinic != null)
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
            SubscriptionPlanId = request.SubscriptionPlanId,
            OnboardingCompleted = true,
            OnboardingCompletedDate = dateTimeProvider.UtcNow
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

        // Create Staff record for owner (links user to clinic)
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ClinicId = clinic.Id,
            IsActive = true,
            HireDate = dateTimeProvider.UtcNow
        };

        db.Staff.Add(staff);

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    public record Request(
        [Required]
        [MaxLength(100)]
        string ClinicName,
        
        [Required]
        Guid SubscriptionPlanId,
        
        [Required]
        [MaxLength(100)]
        string BranchName,
        
        [Required]
        [MaxLength(255)]
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
}
