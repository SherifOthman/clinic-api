using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds 25 contact messages — enough for 3 pages (10/page).
/// Mix of read/unread, different subjects and senders.
/// </summary>
public class DemoContactSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoContactSeeder> _logger;

    public DemoContactSeeder(ApplicationDbContext db, ILogger<DemoContactSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var existing = await _db.Set<ContactMessage>().CountAsync();
        if (existing >= 20) { _logger.LogInformation("Contact messages already seeded — skipping"); return; }

        var now = DateTimeOffset.UtcNow;

        var messages = new[]
        {
            // Unread — recent
            ("Sarah",    "Johnson",   "sarah.j@gmail.com",          "+1-555-0101", "Johnson Medical",    "Pricing Inquiry",           "Hello, I am interested in your Professional plan. Could you provide more details about the pricing and what features are included? We have a clinic with 15 staff members.",                                                                false, now.AddHours(-1)),
            ("Mohammed", "Al-Rashid", "m.rashid@healthclinic.ae",   "+971-50-1234","Al-Rashid Health",   "Technical Support",         "We are experiencing issues with the appointment scheduling feature. When we try to book a time-based appointment, the system shows an error. Please help us resolve this as soon as possible.",                                    false, now.AddHours(-3)),
            ("Emily",    "Chen",      "emily.chen@medcenter.com",   "+1-555-0203", "Pacific Medical",    "Partnership Opportunity",   "We are a network of 5 clinics and are looking for a management solution. We would like to discuss enterprise pricing and custom features for our network.",                                                                         false, now.AddHours(-6)),
            ("Ahmed",    "Hassan",    "ahmed.h@clinic.eg",          "+20-100-2345","",                   "Data Migration Help",       "We are currently using an older system and want to migrate our patient data to your platform. Do you provide data migration services? We have approximately 2000 patient records.",                                                  false, now.AddHours(-8)),
            ("Lisa",     "Martinez",  "lisa.m@dentalcare.com",      "+1-555-0305", "Smile Dental",       "Feature Request",           "It would be great if the system supported dental-specific features like tooth charts and treatment plans. Is this something you are planning to add in future versions?",                                                            false, now.AddHours(-12)),
            ("Omar",     "Khalil",    "omar.k@hospital.sa",         "+966-55-6789","King Faisal Hosp",   "Integration Question",      "Does your system integrate with HL7 FHIR standards? We need to connect with our existing hospital information system and ensure seamless data exchange.",                                                                          false, now.AddHours(-18)),
            ("Fatima",   "Al-Zahra",  "fatima.z@wellness.ae",       "+971-55-9876","Wellness Clinic",    "Billing Question",          "I have a question about the yearly subscription discount. Your website mentions 20% off for annual billing. Is this applied automatically or do I need to contact support?",                                                        false, now.AddHours(-24)),
            ("James",    "Wilson",    "j.wilson@familydoc.com",     "+1-555-0407", "",                   "Solo Practice Inquiry",     "I am a solo practitioner looking for a simple clinic management solution. Is your Starter plan suitable for a single doctor practice with no staff? What are the limitations?",                                                    false, now.AddDays(-2)),
            ("Nour",     "Ibrahim",   "nour.i@pediatric.eg",        "+20-111-3456","Cairo Pediatric",    "Demo Request",              "We would like to schedule a live demo of your system. We are particularly interested in the appointment management and patient history features. When are you available?",                                                          false, now.AddDays(-2)),
            ("David",    "Park",      "d.park@orthoclinic.com",     "+1-555-0509", "Park Orthopedics",   "HIPAA Compliance",          "Is your system HIPAA compliant? We are based in the US and need to ensure all patient data is handled according to HIPAA regulations before we can proceed with a subscription.",                                                  false, now.AddDays(-3)),
            // Read — older
            ("Khalid",   "Al-Otaibi", "k.otaibi@medgroup.sa",       "+966-50-1111","Saudi Med Group",    "Multi-branch Setup",        "We have 8 branches across the country. Can your system handle this? We need centralized reporting and the ability to manage all branches from a single admin account.",                                                            true,  now.AddDays(-4)),
            ("Hana",     "Mostafa",   "hana.m@clinic.eg",           "+20-100-5678","",                   "Staff Permissions",         "How granular are the staff permission settings? We need to restrict certain staff from viewing financial data while allowing them to manage appointments.",                                                                          true,  now.AddDays(-5)),
            ("Robert",   "Taylor",    "r.taylor@medpractice.co.uk", "+44-7911-001","Taylor Practice",    "UK Compliance",             "We are based in the UK. Is your system compliant with UK GDPR and NHS data standards? This is a critical requirement for us before we can consider your platform.",                                                                true,  now.AddDays(-6)),
            ("Mariam",   "Al-Sayed",  "mariam.s@clinic.ae",         "+971-50-2222","Al-Sayed Clinic",    "Arabic Language Support",   "Does your system fully support Arabic language including right-to-left text direction? We serve primarily Arabic-speaking patients and need full RTL support.",                                                                     true,  now.AddDays(-7)),
            ("Michael",  "Brown",     "m.brown@healthplus.com",     "+1-555-0611", "HealthPlus",         "API Access",                "We are interested in the API access feature in the Professional plan. Can you share the API documentation? We want to integrate with our existing EHR system.",                                                                     true,  now.AddDays(-8)),
            ("Rania",    "Fouad",     "rania.f@clinic.eg",          "+20-100-7890","",                   "Subscription Upgrade",      "We are currently on the Starter plan and want to upgrade to Professional. What is the process? Will our existing data be preserved during the upgrade?",                                                                           true,  now.AddDays(-9)),
            ("William",  "Anderson",  "w.anderson@medgroup.com",    "+1-555-0713", "Anderson Medical",   "Training Support",          "Do you provide training for new staff members? We have 20 staff who will need to learn the system. Is there video training material or live training sessions available?",                                                          true,  now.AddDays(-10)),
            ("Noura",    "Al-Ghamdi", "noura.g@clinic.sa",          "+966-55-3333","",                   "Mobile App",                "Is there a mobile app for doctors to view their appointments on the go? Our doctors frequently need to check their schedule from their phones.",                                                                                    true,  now.AddDays(-11)),
            ("Jennifer", "Davis",     "j.davis@familyhealth.com",   "+1-555-0815", "Family Health",      "Cancellation Policy",       "What is your cancellation policy? If we decide to cancel our subscription, how much notice do we need to give and will we receive a refund for unused months?",                                                                   true,  now.AddDays(-12)),
            ("Tarek",    "Samir",     "tarek.s@clinic.eg",          "+20-100-9012","",                   "Custom Branding",           "We are interested in the custom branding feature. Can we add our clinic logo and use our own color scheme throughout the system? Is this available on all plans?",                                                                 true,  now.AddDays(-13)),
            ("Amanda",   "White",     "a.white@dentalgroup.com",    "+1-555-0917", "White Dental Group", "Inventory Management",      "We need inventory management for dental supplies. Does your system track stock levels and send alerts when supplies are running low? Is this included in the Professional plan?",                                                  true,  now.AddDays(-14)),
            ("Faisal",   "Al-Harbi",  "faisal.h@medcenter.sa",      "+966-50-4444","Al-Harbi Medical",   "Reporting Features",        "Can you tell me more about the reporting features? We need monthly reports on patient visits, revenue, and staff performance. Can these be exported to Excel or PDF?",                                                             true,  now.AddDays(-15)),
            ("Sherif",   "Adel",      "sherif.a@clinic.eg",         "+20-100-3456","",                   "Patient Portal",            "Do you have a patient portal where patients can book appointments online and view their medical history? This is a feature our patients have been requesting.",                                                                     true,  now.AddDays(-18)),
            ("Hessa",    "Al-Dosari", "hessa.d@clinic.ae",          "+971-55-5555","Al-Dosari Clinic",   "Telemedicine Support",      "Does your platform support telemedicine or video consultations? With the increasing demand for remote healthcare, this has become essential for our practice.",                                                                     true,  now.AddDays(-21)),
            ("Walid",    "Fathy",     "walid.f@clinic.eg",          "+20-100-6789","",                   "General Feedback",          "I have been using your system for 3 months now and I am very satisfied. The appointment management is excellent. One suggestion: it would be great to have SMS reminders for patients.",                                           true,  now.AddDays(-25)),
        };

        var list = messages.Select(m => new ContactMessage
        {
            FirstName = m.Item1,
            LastName  = m.Item2,
            Email     = m.Item3,
            Phone     = string.IsNullOrEmpty(m.Item4) ? null : m.Item4,
            Company   = string.IsNullOrEmpty(m.Item5) ? null : m.Item5,
            Subject   = m.Item6,
            Message   = m.Item7,
            IsRead    = m.Item8,
            CreatedAt = m.Item9,
        }).ToList();

        _db.Set<ContactMessage>().AddRange(list);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo contact messages", list.Count);
    }
}
