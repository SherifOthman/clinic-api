using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds testimonials from multiple clinic owners.
/// Creates 3 extra demo clinic owners with their clinics so we have enough
/// testimonials to test the reviews page pagination (12/page).
/// Mix of approved/pending, different ratings.
/// </summary>
public class DemoTestimonialsSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DemoTestimonialsSeeder> _logger;

    public DemoTestimonialsSeeder(
        ApplicationDbContext db,
        UserManager<User> userManager,
        ILogger<DemoTestimonialsSeeder> logger)
    {
        _db          = db;
        _userManager = userManager;
        _logger      = logger;
    }

    public async Task SeedAsync(DemoClinicContext ctx)
    {
        var existing = await _db.Set<Testimonial>().IgnoreQueryFilters().CountAsync();
        if (existing >= 10) { _logger.LogInformation("Testimonials already seeded — skipping"); return; }

        var plan = await _db.Set<SubscriptionPlan>().FirstOrDefaultAsync(p => p.IsActive);
        if (plan is null) return;

        var now = DateTimeOffset.UtcNow;

        // ── 1. Testimonial from the main demo owner ───────────────────────────
        await EnsureTestimonialAsync(ctx.OwnerUserId, ctx.ClinicId,
            "ClinicCare has completely transformed how we manage our clinic. The appointment scheduling is intuitive and our staff adapted to it within days. Patient management is seamless and the reporting features give us great insights into our practice performance.",
            5, true, now.AddDays(-45), ctx.OwnerUserId);

        // ── 2. Extra clinic owners with testimonials ──────────────────────────
        var extras = new[]
        {
            ("owner2@demo.com", "owner2", "Dr. Sarah Mitchell",   "+447911200001", "Mitchell Clinic",
             "The system is very user-friendly and the support team is responsive. We especially love the multi-branch support which allows us to manage all our locations from one dashboard. Highly recommended for growing clinics.",
             5, true, now.AddDays(-38)),

            ("owner3@demo.com", "owner3", "Dr. Khalid Al-Rashid", "+966502000001", "Al-Rashid Medical Center",
             "Good system overall. The patient management and appointment features work well. Would love to see more customization options for reports. The mobile experience could also be improved but overall it is a solid platform.",
             4, true, now.AddDays(-30)),

            ("owner4@demo.com", "owner4", "Dr. Emily Chen",       "+447911200002", "Pacific Health Clinic",
             "We switched from a legacy system and the migration was smooth. The queue-based appointment system is perfect for our high-volume clinic. Staff training took less than a week. Very impressed with the overall quality.",
             5, true, now.AddDays(-22)),

            ("owner5@demo.com", "owner5", "Dr. Ahmed Mansour",    "+201002000001", "Mansour Clinic",
             "Excellent value for money. The Starter plan covers everything a small clinic needs. The billing and invoicing features save us hours every week. Customer support is always helpful and responsive.",
             5, true, now.AddDays(-18)),

            ("owner6@demo.com", "owner6", "Dr. Jennifer Davis",   "+15550200001",  "Davis Family Practice",
             "The appointment reminder system has significantly reduced our no-show rate. Patients appreciate the automated reminders. The staff management features are also very well designed.",
             4, true, now.AddDays(-14)),

            ("owner7@demo.com", "owner7", "Dr. Omar Al-Farsi",    "+971502000001", "Al-Farsi Specialty Clinic",
             "We have been using ClinicCare for 6 months and it has exceeded our expectations. The analytics dashboard gives us valuable insights into our clinic performance. The Arabic language support is excellent.",
             5, true, now.AddDays(-10)),

            ("owner8@demo.com", "owner8", "Dr. Lisa Martinez",    "+15550200002",  "Martinez Dental",
             "Decent system with good core features. The appointment management works well for our dental practice. Would benefit from more dental-specific features but the general clinic management is solid.",
             3, true, now.AddDays(-7)),

            ("owner9@demo.com", "owner9", "Dr. Mariam Al-Sayed",  "+971502000002", "Al-Sayed Women's Clinic",
             "The patient privacy features are excellent and HIPAA compliance gives us peace of mind. The system is reliable and we have had minimal downtime. Highly recommend for specialty clinics.",
             5, true, now.AddDays(-5)),

            // Pending (not yet approved)
            ("owner10@demo.com","owner10","Dr. Robert Taylor",    "+447911200003", "Taylor Medical Practice",
             "Just started using the system last month. So far the onboarding was smooth and the interface is clean. Looking forward to exploring all the features as we grow our practice.",
             4, false, now.AddDays(-3)),

            ("owner11@demo.com","owner11","Dr. Hana Mostafa",     "+201002000002", "Mostafa Clinic",
             "The system has good potential. We are still in the early stages of using it but the appointment scheduling and patient registration are working well. Will update this review after more experience.",
             4, false, now.AddDays(-2)),

            ("owner12@demo.com","owner12","Dr. Faisal Al-Harbi",  "+966502000002", "Al-Harbi Specialist Center",
             "Very promising platform. The subscription plans are reasonably priced and the features match what we need. The customer support team was very helpful during our onboarding process.",
             5, false, now.AddDays(-1)),
        };

        foreach (var (email, username, fullName, phone, clinicName, text, rating, approved, createdAt) in extras)
        {
            var user = await EnsureOwnerAsync(email, username, fullName, phone);
            if (user is null) continue;

            var clinic = await EnsureClinicAsync(user.Id, clinicName, plan.Id);
            await EnsureTestimonialAsync(user.Id, clinic.Id, text, rating, approved, createdAt, user.Id);
        }

        _logger.LogInformation("Demo testimonials seeded");
    }

    private async Task<User?> EnsureOwnerAsync(string email, string username, string fullName, string phone)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null) return existing;

        var user = new User
        {
            UserName       = username,
            Email          = email,
            PhoneNumber    = phone,
            EmailConfirmed = true,
            FullName       = fullName,
            Gender         = Domain.Enums.Gender.Male,
        };

        var result = await _userManager.CreateAsync(user, "DemoOwner123!");
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create demo owner {Email}", email);
            return null;
        }

        await _userManager.AddToRoleAsync(user, Domain.Entities.UserRoles.ClinicOwner);
        return user;
    }

    private async Task<Clinic> EnsureClinicAsync(Guid ownerId, string name, Guid planId)
    {
        var existing = await _db.Set<Clinic>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerId);

        if (existing is not null) return existing;

        var clinic = new Clinic
        {
            Name                = name,
            OwnerUserId         = ownerId,
            SubscriptionPlanId  = planId,
            OnboardingCompleted = true,
            IsActive            = true,
            CountryCode         = "EG",
        };
        _db.Set<Clinic>().Add(clinic);

        _db.Set<ClinicSubscription>().Add(new ClinicSubscription
        {
            ClinicId           = clinic.Id,
            SubscriptionPlanId = planId,
            Status             = Domain.Enums.SubscriptionStatus.Active,
            StartDate          = DateTimeOffset.UtcNow.AddMonths(-3),
            EndDate            = DateTimeOffset.UtcNow.AddMonths(9),
            AutoRenew          = true,
        });

        await _db.SaveChangesAsync();
        return clinic;
    }

    private async Task EnsureTestimonialAsync(
        Guid userId, Guid clinicId, string text, int rating,
        bool isApproved, DateTimeOffset createdAt, Guid createdBy)
    {
        var exists = await _db.Set<Testimonial>().IgnoreQueryFilters()
            .AnyAsync(t => t.UserId == userId && t.ClinicId == clinicId);

        if (exists) return;

        _db.Set<Testimonial>().Add(new Testimonial
        {
            ClinicId   = clinicId,
            UserId     = userId,
            Text       = text,
            Rating     = rating,
            IsApproved = isApproved,
            CreatedAt  = createdAt,
            UpdatedAt  = createdAt,
            CreatedBy  = createdBy,
            UpdatedBy  = createdBy,
        });

        await _db.SaveChangesAsync();
    }
}
