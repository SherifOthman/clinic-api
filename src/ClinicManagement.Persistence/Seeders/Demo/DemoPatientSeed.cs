using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds 25 realistic patients with phones and chronic diseases.
/// Covers all blood types, both genders, various ages.
/// </summary>
public class DemoPatientSeed
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoPatientSeed> _logger;

    public DemoPatientSeed(ApplicationDbContext db, ILogger<DemoPatientSeed> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<List<Patient>> SeedAsync(Guid clinicId)
    {
        var data = new[]
        {
            // name,                  gender,   dob,          phone,            blood,  diseases
            ("Ahmed Hassan",          "Male",   "1985-03-15", "+201001234567",  "A+",   new[]{"Diabetes Type 2","Hypertension"}),
            ("Fatima Al-Zahra",       "Female", "1990-07-22", "+201112345678",  "B+",   new[]{"Asthma"}),
            ("Mohamed Ali",           "Male",   "1978-11-08", "+201223456789",  "O+",   new[]{"Hypertension","Coronary Artery Disease"}),
            ("Sara Ibrahim",          "Female", "1995-02-14", "+201334567890",  "AB+",  new string[0]),
            ("Khaled Mahmoud",        "Male",   "1982-09-30", "+201445678901",  "A-",   new[]{"Diabetes Type 2"}),
            ("Nour El-Din",           "Female", "1998-05-18", "+201556789012",  "B-",   new string[0]),
            ("Omar Sherif",           "Male",   "1975-12-03", "+201667890123",  "O-",   new[]{"Chronic Kidney Disease","Hypertension"}),
            ("Layla Hassan",          "Female", "1992-08-25", "+201778901234",  "AB-",  new[]{"Hypothyroidism"}),
            ("Youssef Kamal",         "Male",   "1988-04-10", "+201889012345",  "A+",   new string[0]),
            ("Hana Mostafa",          "Female", "1993-06-28", "+201990123456",  "B+",   new[]{"Asthma","Allergic Rhinitis"}),
            ("Tarek Nasser",          "Male",   "1970-01-17", "+201001122334",  "O+",   new[]{"Diabetes Type 2","Hypertension","Coronary Artery Disease"}),
            ("Rania Fawzy",           "Female", "1987-10-05", "+201112233445",  "A+",   new string[0]),
            ("Amr Saad",              "Male",   "1996-03-22", "+201223344556",  "AB+",  new string[0]),
            ("Dina Khalil",           "Female", "1983-08-14", "+201334455667",  "B+",   new[]{"Hypothyroidism","Hypertension"}),
            ("Hassan Ramadan",        "Male",   "1979-12-30", "+201445566778",  "O+",   new[]{"Chronic Obstructive Pulmonary Disease"}),
            ("Mona Adel",             "Female", "1991-05-07", "+201556677889",  "A-",   new string[0]),
            ("Sherif Gamal",          "Male",   "1986-09-19", "+201667788990",  "B+",   new[]{"Diabetes Type 2"}),
            ("Aya Mahmoud",           "Female", "1994-02-28", "+201778899001",  "O+",   new string[0]),
            ("Karim Farouk",          "Male",   "1977-07-11", "+201889900112",  "A+",   new[]{"Hypertension","Atrial Fibrillation"}),
            ("Noha Sayed",            "Female", "1989-11-23", "+201990011223",  "AB+",  new string[0]),
            ("Mahmoud Tawfik",        "Male",   "1965-04-08", "+201001234568",  "O+",   new[]{"Diabetes Type 2","Hypertension","Chronic Kidney Disease"}),
            ("Samira Abdel-Aziz",     "Female", "2001-09-15", "+201112345679",  "B+",   new string[0]),
            ("Ibrahim El-Sayed",      "Male",   "1958-06-20", "+201223456780",  "A+",   new[]{"Coronary Artery Disease","Hypertension"}),
            ("Nadia Fouad",           "Female", "1972-12-11", "+201334567891",  "AB-",  new[]{"Hypothyroidism","Diabetes Type 2"}),
            ("Walid Mansour",         "Male",   "2005-03-25", "+201445678902",  "O-",   new string[0]),
        };

        // Load chronic diseases for linking
        var diseases = await _db.Set<ChronicDisease>().ToListAsync();
        var diseaseMap = diseases.ToDictionary(d => d.NameEn, d => d.Id, StringComparer.OrdinalIgnoreCase);

        var patients = new List<Patient>();

        for (var i = 0; i < data.Length; i++)
        {
            var (name, gender, dob, phone, blood, diseaseNames) = data[i];

            var person = new Person
            {
                FullName    = name,
                Gender      = gender == "Male" ? Gender.Male : Gender.Female,
                DateOfBirth = DateOnly.Parse(dob),
            };

            var patient = new Patient
            {
                ClinicId    = clinicId,
                PatientCode = (i + 1).ToString("D4"),
                BloodType   = blood switch
                {
                    "A+"  => BloodType.APositive,  "A-"  => BloodType.ANegative,
                    "B+"  => BloodType.BPositive,  "B-"  => BloodType.BNegative,
                    "AB+" => BloodType.ABPositive, "AB-" => BloodType.ABNegative,
                    "O+"  => BloodType.OPositive,  "O-"  => BloodType.ONegative,
                    _     => (BloodType?)null,
                },
                PersonId  = person.Id,
                Person    = person,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-(data.Length - i) * 4),
            };

            _db.Set<Person>().Add(person);
            _db.Set<Patient>().Add(patient);
            patients.Add(patient);

            // Phone
            _db.Set<PatientPhone>().Add(new PatientPhone
            {
                PatientId      = patient.Id,
                PhoneNumber    = phone,
                NationalNumber = phone.TrimStart('+').Substring(2), // strip country code
            });

            // Chronic diseases
            foreach (var diseaseName in diseaseNames)
            {
                if (diseaseMap.TryGetValue(diseaseName, out var diseaseId))
                    _db.Set<PatientChronicDisease>().Add(new PatientChronicDisease
                    {
                        PatientId       = patient.Id,
                        ChronicDiseaseId = diseaseId,
                    });
            }
        }

        // Update PatientCounter
        var counter = await _db.Set<PatientCounter>().FirstOrDefaultAsync(c => c.ClinicId == clinicId);
        if (counter is not null) counter.LastValue = data.Length;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo patients", patients.Count);
        return patients;
    }
}
