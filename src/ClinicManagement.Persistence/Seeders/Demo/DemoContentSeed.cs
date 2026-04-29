using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds testimonials and contact messages for the marketing website.
/// </summary>
public class DemoContentSeed
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoContentSeed> _logger;

    public DemoContentSeed(ApplicationDbContext db, ILogger<DemoContentSeed> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(Guid clinicId, Guid ownerUserId, string clinicName)
    {
        // ── Testimonials ──────────────────────────────────────────────────────
        _db.Set<Testimonial>().AddRange(
            new Testimonial
            {
                ClinicId   = clinicId,
                UserId     = ownerUserId,
                ClinicName = clinicName,
                AuthorName = "Dr. John Smith",
                Position   = "General Practitioner",
                Text       = "ClinicCare has completely transformed how we manage our clinic. The appointment system is intuitive, the patient records are comprehensive, and our staff adapted within days. Highly recommended for any clinic looking to modernize.",
                Rating     = 5,
                IsApproved = true,
            }
        );

        // ── Contact messages ──────────────────────────────────────────────────
        _db.Set<ContactMessage>().AddRange(
            new ContactMessage
            {
                FirstName = "Sherif",
                LastName  = "Ali",
                Email     = "sherif.ali@example.com",
                Phone     = "+201001234567",
                Company   = "Al-Nour Medical Center",
                Subject   = "Pricing inquiry — Growing Clinic plan",
                Message   = "Hello, I'm interested in the Growing Clinic plan. Could you tell me more about the multi-branch support and whether we can have different doctors at each branch? We currently have 2 locations in Cairo.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                IsRead    = true,
            },
            new ContactMessage
            {
                FirstName = "Mona",
                LastName  = "Kamal",
                Email     = "mona.kamal@example.com",
                Phone     = "+201112345678",
                Subject   = "Google login not working",
                Message   = "I'm having trouble logging in with my Google account. It redirects me back to the login page without any error message. I've tried on Chrome and Firefox. Please help.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                IsRead    = false,
            },
            new ContactMessage
            {
                FirstName = "Tarek",
                LastName  = "Nour",
                Email     = "tarek.nour@example.com",
                Company   = "Nour Cardiology Clinic",
                Subject   = "Feature request: appointment reminders",
                Message   = "Would it be possible to add SMS or WhatsApp reminders for patients before their appointments? This would significantly reduce no-shows at our clinic. We see about 50 patients per day.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                IsRead    = false,
            },
            new ContactMessage
            {
                FirstName = "Heba",
                LastName  = "Mansour",
                Email     = "heba.mansour@example.com",
                Phone     = "+201334567890",
                Subject   = "Trial period extension request",
                Message   = "We've been using ClinicCare for 10 days and love it, but our IT team needs more time to migrate our existing patient records. Could we get a 2-week extension on the trial?",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-6),
                IsRead    = false,
            },
            new ContactMessage
            {
                FirstName = "Ahmed",
                LastName  = "Farouk",
                Email     = "ahmed.farouk@example.com",
                Subject   = "Integration with lab systems",
                Message   = "Do you have any plans to integrate with laboratory information systems? We work with 3 external labs and it would be great to receive results directly in the patient record.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                IsRead    = false,
            }
        );

        await _db.SaveChangesAsync();
        _logger.LogInformation("Demo content (testimonials, contact messages) seeded");
    }
}
