using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds 50 patients — enough to trigger pagination (default page size 10).
/// Mix of genders, blood types, ages, and chronic diseases.
/// </summary>
public class DemoPatientsSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoPatientsSeeder> _logger;

    public DemoPatientsSeeder(ApplicationDbContext db, ILogger<DemoPatientsSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(DemoClinicContext ctx)
    {
        var existing = await _db.Set<Patient>().IgnoreQueryFilters()
            .CountAsync(p => p.ClinicId == ctx.ClinicId);

        if (existing >= 50) { _logger.LogInformation("Patients already seeded — skipping"); return; }

        // Ensure patient counter exists
        var counter = await _db.Set<PatientCounter>()
            .FirstOrDefaultAsync(c => c.ClinicId == ctx.ClinicId);
        if (counter is null)
        {
            counter = new PatientCounter { ClinicId = ctx.ClinicId, LastValue = 0 };
            _db.Set<PatientCounter>().Add(counter);
            await _db.SaveChangesAsync();
        }

        var chronicDiseases = await _db.Set<ChronicDisease>().Take(10).ToListAsync();

        var patients = BuildPatients(ctx.ClinicId, ctx.OwnerUserId, counter.LastValue);

        foreach (var (patient, phones, diseaseIndices) in patients)
        {
            _db.Set<Patient>().Add(patient);
            foreach (var phone in phones)
                _db.Set<PatientPhone>().Add(phone);

            foreach (var idx in diseaseIndices)
            {
                if (idx < chronicDiseases.Count)
                {
                    _db.Set<PatientChronicDisease>().Add(new PatientChronicDisease
                    {
                        PatientId        = patient.Id,
                        ChronicDiseaseId = chronicDiseases[idx].Id,
                    });
                }
            }

            counter.LastValue++;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo patients", patients.Count);
    }

    private static List<(Patient, List<PatientPhone>, int[])> BuildPatients(
        Guid clinicId, Guid createdBy, int startCode)
    {
        var now = DateTimeOffset.UtcNow;

        // 50 realistic patients — mix of Arabic and Western names
        var data = new[]
        {
            ("Ahmed Hassan",        Gender.Male,   new DateOnly(1985, 3, 15),  BloodType.APositive,  "+201001234001", new[] {0}),
            ("Fatima Al-Zahra",     Gender.Female, new DateOnly(1990, 7, 22),  BloodType.BPositive,  "+201001234002", new[] {1,2}),
            ("Mohammed Ali",        Gender.Male,   new DateOnly(1978, 11, 5),  BloodType.OPositive,  "+201001234003", new[] {0,3}),
            ("Nour Ibrahim",        Gender.Female, new DateOnly(1995, 1, 30),  BloodType.ABPositive, "+201001234004", Array.Empty<int>()),
            ("Omar Khalil",         Gender.Male,   new DateOnly(1982, 6, 18),  BloodType.ANegative,  "+201001234005", new[] {2}),
            ("Sara Ahmed",          Gender.Female, new DateOnly(1988, 9, 12),  BloodType.ONegative,  "+201001234006", new[] {1}),
            ("Youssef Mansour",     Gender.Male,   new DateOnly(1975, 4, 25),  BloodType.BNegative,  "+201001234007", new[] {0,1,2}),
            ("Layla Hassan",        Gender.Female, new DateOnly(1993, 12, 8),  BloodType.APositive,  "+201001234008", Array.Empty<int>()),
            ("Karim Nasser",        Gender.Male,   new DateOnly(1980, 8, 3),   BloodType.OPositive,  "+201001234009", new[] {3}),
            ("Hana Mostafa",        Gender.Female, new DateOnly(1997, 2, 14),  BloodType.BPositive,  "+201001234010", Array.Empty<int>()),
            ("Tarek Samir",         Gender.Male,   new DateOnly(1970, 5, 20),  BloodType.ABNegative, "+201001234011", new[] {0,4}),
            ("Rania Fouad",         Gender.Female, new DateOnly(1986, 10, 7),  BloodType.APositive,  "+201001234012", new[] {1}),
            ("Amr Sayed",           Gender.Male,   new DateOnly(1992, 3, 28),  BloodType.OPositive,  "+201001234013", Array.Empty<int>()),
            ("Dina Ramadan",        Gender.Female, new DateOnly(1989, 7, 16),  BloodType.BPositive,  "+201001234014", new[] {2,3}),
            ("Sherif Adel",         Gender.Male,   new DateOnly(1983, 1, 9),   BloodType.ANegative,  "+201001234015", new[] {0}),
            ("Mona Gamal",          Gender.Female, new DateOnly(1994, 11, 23), BloodType.OPositive,  "+201001234016", Array.Empty<int>()),
            ("Walid Fathy",         Gender.Male,   new DateOnly(1977, 6, 11),  BloodType.APositive,  "+201001234017", new[] {1,4}),
            ("Eman Tawfik",         Gender.Female, new DateOnly(1991, 4, 5),   BloodType.BNegative,  "+201001234018", new[] {0}),
            ("Hossam Badr",         Gender.Male,   new DateOnly(1987, 9, 30),  BloodType.OPositive,  "+201001234019", Array.Empty<int>()),
            ("Yasmin Salah",        Gender.Female, new DateOnly(1996, 2, 17),  BloodType.ABPositive, "+201001234020", new[] {2}),
            ("James Wilson",        Gender.Male,   new DateOnly(1979, 7, 4),   BloodType.APositive,  "+447911123001", new[] {0,1}),
            ("Emily Chen",          Gender.Female, new DateOnly(1990, 12, 19), BloodType.BPositive,  "+447911123002", Array.Empty<int>()),
            ("David Park",          Gender.Male,   new DateOnly(1984, 3, 8),   BloodType.OPositive,  "+447911123003", new[] {3}),
            ("Sarah Johnson",       Gender.Female, new DateOnly(1993, 8, 25),  BloodType.ANegative,  "+447911123004", new[] {1}),
            ("Michael Brown",       Gender.Male,   new DateOnly(1976, 5, 13),  BloodType.ONegative,  "+447911123005", new[] {0,2}),
            ("Lisa Martinez",       Gender.Female, new DateOnly(1988, 10, 2),  BloodType.BPositive,  "+447911123006", Array.Empty<int>()),
            ("Robert Taylor",       Gender.Male,   new DateOnly(1981, 1, 27),  BloodType.APositive,  "+447911123007", new[] {4}),
            ("Jennifer Davis",      Gender.Female, new DateOnly(1995, 6, 14),  BloodType.OPositive,  "+447911123008", Array.Empty<int>()),
            ("William Anderson",    Gender.Male,   new DateOnly(1973, 11, 6),  BloodType.ABPositive, "+447911123009", new[] {0,1,3}),
            ("Amanda White",        Gender.Female, new DateOnly(1992, 4, 21),  BloodType.BNegative,  "+447911123010", new[] {2}),
            ("Ali Al-Rashid",       Gender.Male,   new DateOnly(1986, 8, 9),   BloodType.OPositive,  "+966501234001", new[] {0}),
            ("Mariam Al-Sayed",     Gender.Female, new DateOnly(1991, 3, 16),  BloodType.APositive,  "+966501234002", Array.Empty<int>()),
            ("Khalid Al-Otaibi",    Gender.Male,   new DateOnly(1978, 12, 3),  BloodType.BPositive,  "+966501234003", new[] {1,2}),
            ("Noura Al-Ghamdi",     Gender.Female, new DateOnly(1994, 7, 28),  BloodType.OPositive,  "+966501234004", new[] {0}),
            ("Faisal Al-Harbi",     Gender.Male,   new DateOnly(1982, 2, 11),  BloodType.ANegative,  "+966501234005", new[] {3,4}),
            ("Hessa Al-Dosari",     Gender.Female, new DateOnly(1989, 9, 24),  BloodType.ABPositive, "+966501234006", Array.Empty<int>()),
            ("Abdulaziz Al-Qahtani",Gender.Male,   new DateOnly(1975, 5, 7),   BloodType.ONegative,  "+966501234007", new[] {0,1}),
            ("Reem Al-Mutairi",     Gender.Female, new DateOnly(1997, 1, 19),  BloodType.BPositive,  "+966501234008", Array.Empty<int>()),
            ("Turki Al-Shehri",     Gender.Male,   new DateOnly(1983, 6, 30),  BloodType.APositive,  "+966501234009", new[] {2}),
            ("Dalal Al-Zahrani",    Gender.Female, new DateOnly(1990, 11, 12), BloodType.OPositive,  "+966501234010", new[] {1}),
            ("Hassan Mahmoud",      Gender.Male,   new DateOnly(1980, 4, 5),   BloodType.BNegative,  "+201001234041", new[] {0,3}),
            ("Samira Younis",       Gender.Female, new DateOnly(1987, 8, 18),  BloodType.APositive,  "+201001234042", Array.Empty<int>()),
            ("Mahmoud Farouk",      Gender.Male,   new DateOnly(1974, 2, 23),  BloodType.OPositive,  "+201001234043", new[] {1,4}),
            ("Aya Khaled",          Gender.Female, new DateOnly(1998, 6, 7),   BloodType.ABNegative, "+201001234044", Array.Empty<int>()),
            ("Bassem Nabil",        Gender.Male,   new DateOnly(1985, 10, 14), BloodType.BPositive,  "+201001234045", new[] {0}),
            ("Ghada Samir",         Gender.Female, new DateOnly(1992, 3, 29),  BloodType.OPositive,  "+201001234046", new[] {2}),
            ("Adel Hamdy",          Gender.Male,   new DateOnly(1969, 7, 8),   BloodType.APositive,  "+201001234047", new[] {0,1,2}),
            ("Nadia Fawzy",         Gender.Female, new DateOnly(1996, 12, 21), BloodType.BNegative,  "+201001234048", Array.Empty<int>()),
            ("Samir Lotfy",         Gender.Male,   new DateOnly(1977, 5, 3),   BloodType.OPositive,  "+201001234049", new[] {3}),
            ("Heba Ashraf",         Gender.Female, new DateOnly(1993, 9, 16),  BloodType.ANegative,  "+201001234050", new[] {1}),
        };

        var result = new List<(Patient, List<PatientPhone>, int[])>();
        int codeNum = startCode + 1;

        foreach (var (name, gender, dob, blood, phone, diseases) in data)
        {
            var daysAgo = Random.Shared.Next(1, 365);
            var patient = new Patient
            {
                ClinicId    = clinicId,
                PatientCode = codeNum.ToString("D4"),
                FullName    = name,
                Gender      = gender,
                DateOfBirth = dob,
                BloodType   = blood,
                CreatedAt   = now.AddDays(-daysAgo),
                UpdatedAt   = now.AddDays(-daysAgo),
                CreatedBy   = createdBy,
                UpdatedBy   = createdBy,
            };

            var phones = new List<PatientPhone>
            {
                new() { PatientId = patient.Id, PhoneNumber = phone, NationalNumber = phone[(phone.LastIndexOf('+') + 3)..] }
            };

            result.Add((patient, phones, diseases));
            codeNum++;
        }

        return result;
    }
}
