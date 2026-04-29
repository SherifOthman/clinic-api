using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Persistence.Seeders;

public class DemoUsersSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DemoUsersSeedService> _logger;
    private readonly SeedOptions _options;

    public DemoUsersSeedService(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DemoUsersSeedService> logger,
        IOptions<SeedOptions> options)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        if (!_options.SeedDemoUsers)
        {
            _logger.LogInformation("Demo user seeding is disabled");
            return;
        }

        await SeedSuperAdminAsync();
        var clinic = await SeedClinicOwnerAsync();
        if (clinic is null) return;
        await SeedDoctorAsync(clinic);
        await SeedReceptionistAsync(clinic);
        await SeedAppointmentDataAsync(clinic);
    }

    private async Task SeedSuperAdminAsync()
    {
        var opts = _options.SuperAdmin;
        if (await _userManager.FindByEmailAsync(opts.Email) != null) return;

        var person = new Person { FullName = "System Administrator", Gender = Gender.Male };
        var user = new User
        {
            UserName = "superadmin",
            Email = opts.Email,
            PhoneNumber = "+966500000000",
            EmailConfirmed = true,
            PersonId = person.Id,
            Person = person,
        };
        var result = await _userManager.CreateAsync(user, opts.Password);
        if (!result.Succeeded) { LogError("SuperAdmin", result); return; }
        await _userManager.AddToRoleAsync(user, UserRoles.SuperAdmin);
        _logger.LogInformation("SuperAdmin seeded: {Email}", opts.Email);
    }

    private async Task<Clinic?> SeedClinicOwnerAsync()
    {
        var opts = _options.ClinicOwner;
        var owner = await _userManager.FindByEmailAsync(opts.Email);

        if (owner == null)
        {
            var person = new Person { FullName = "John Smith", Gender = Gender.Male };
            owner = new User
            {
                UserName = "owner",
                Email = opts.Email,
                PhoneNumber = "+1234567890",
                EmailConfirmed = true,
                PersonId = person.Id,
                Person = person,
            };
            var result = await _userManager.CreateAsync(owner, opts.Password);
            if (!result.Succeeded) { LogError("ClinicOwner", result); return null; }
            await _userManager.AddToRoleAsync(owner, UserRoles.ClinicOwner);
            await _userManager.AddToRoleAsync(owner, UserRoles.Doctor);
            _logger.LogInformation("ClinicOwner seeded: {Email}", opts.Email);
        }

        var existing = await _context.Set<Clinic>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(c => c.OwnerUserId == owner.Id);
        if (existing != null) return existing;

        var basicPlan = await _context.Set<SubscriptionPlan>().FirstOrDefaultAsync(p => p.Name == "Basic");
        if (basicPlan == null) { _logger.LogError("Basic plan not found"); return null; }

        var generalPractice = await _context.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "General Practice");

        var clinic = new Clinic
        {
            Name = "Demo Medical Clinic",
            OwnerUserId = owner.Id,
            SubscriptionPlanId = basicPlan.Id,
            OnboardingCompleted = true,
            IsActive = true,
            SubscriptionStartDate = DateTimeOffset.UtcNow,
            SubscriptionEndDate = DateTimeOffset.UtcNow.AddMonths(1),
            TrialEndDate = DateTimeOffset.UtcNow.AddDays(14),
        };
        _context.Set<Clinic>().Add(clinic);

        _context.Set<ClinicBranch>().Add(new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = "Main Branch",
            AddressLine = "123 Medical Street, Downtown",
            StateGeonameId = 360630,
            CityGeonameId = 360630,
            IsMainBranch = true,
            IsActive = true,
        });

        _context.Set<ClinicSubscription>().Add(new ClinicSubscription
        {
            ClinicId = clinic.Id,
            SubscriptionPlanId = basicPlan.Id,
            Status = SubscriptionStatus.Trial,
            StartDate = DateTimeOffset.UtcNow,
            TrialEndDate = DateTimeOffset.UtcNow.AddDays(14),
            AutoRenew = true,
        });

        var ownerMember = new ClinicMember
        {
            PersonId = owner.PersonId,
            UserId = owner.Id,
            ClinicId = clinic.Id,
            Role = ClinicMemberRole.Owner,
            IsActive = true,
        };
        _context.Set<ClinicMember>().Add(ownerMember);
        _context.Set<DoctorInfo>().Add(new DoctorInfo
        {
            ClinicMemberId = ownerMember.Id,
            SpecializationId = generalPractice?.Id,
        });

        // Seed default permissions for owner
        var ownerPermissions = DefaultPermissions.ForRole(ClinicMemberRole.Owner)
            .Select(p => new ClinicMemberPermission { ClinicMemberId = ownerMember.Id, Permission = p });
        _context.Set<ClinicMemberPermission>().AddRange(ownerPermissions);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Demo clinic seeded (ClinicId: {Id})", clinic.Id);
        return clinic;
    }

    private async Task SeedDoctorAsync(Clinic clinic)
    {
        var opts = _options.Doctor;
        var doctor = await _userManager.FindByEmailAsync(opts.Email);

        if (doctor == null)
        {
            var person = new Person { FullName = "Sarah Johnson", Gender = Gender.Female };
            doctor = new User
            {
                UserName = "doctor",
                Email = opts.Email,
                PhoneNumber = "+1234567891",
                EmailConfirmed = true,
                PersonId = person.Id,
                Person = person,
            };
            var result = await _userManager.CreateAsync(doctor, opts.Password);
            if (!result.Succeeded) { LogError("Doctor", result); return; }
            await _userManager.AddToRoleAsync(doctor, UserRoles.Doctor);
            _logger.LogInformation("Doctor seeded: {Email}", opts.Email);
        }

        var alreadyMember = await _context.Set<ClinicMember>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AnyAsync(m => m.UserId == doctor.Id && m.ClinicId == clinic.Id);
        if (alreadyMember) return;

        var cardiology = await _context.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "Cardiology");
        var member = new ClinicMember
        {
            PersonId = doctor.PersonId,
            UserId = doctor.Id,
            ClinicId = clinic.Id,
            Role = ClinicMemberRole.Doctor,
            IsActive = true,
        };
        _context.Set<ClinicMember>().Add(member);
        _context.Set<DoctorInfo>().Add(new DoctorInfo { ClinicMemberId = member.Id, SpecializationId = cardiology?.Id });

        var doctorPermissions = DefaultPermissions.ForRole(ClinicMemberRole.Doctor)
            .Select(p => new ClinicMemberPermission { ClinicMemberId = member.Id, Permission = p });
        _context.Set<ClinicMemberPermission>().AddRange(doctorPermissions);

        await _context.SaveChangesAsync();
    }

    private async Task SeedReceptionistAsync(Clinic clinic)
    {
        var opts = _options.Receptionist;
        var receptionist = await _userManager.FindByEmailAsync(opts.Email);

        if (receptionist == null)
        {
            var person = new Person { FullName = "Emily Davis", Gender = Gender.Female };
            receptionist = new User
            {
                UserName = "receptionist",
                Email = opts.Email,
                PhoneNumber = "+1234567892",
                EmailConfirmed = true,
                PersonId = person.Id,
                Person = person,
            };
            var result = await _userManager.CreateAsync(receptionist, opts.Password);
            if (!result.Succeeded) { LogError("Receptionist", result); return; }
            await _userManager.AddToRoleAsync(receptionist, UserRoles.Receptionist);
            _logger.LogInformation("Receptionist seeded: {Email}", opts.Email);
        }

        var alreadyMember = await _context.Set<ClinicMember>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AnyAsync(m => m.UserId == receptionist.Id && m.ClinicId == clinic.Id);
        if (alreadyMember) return;

        var receptionistMember = new ClinicMember
        {
            PersonId = receptionist.PersonId,
            UserId = receptionist.Id,
            ClinicId = clinic.Id,
            Role = ClinicMemberRole.Receptionist,
            IsActive = true,
        };
        _context.Set<ClinicMember>().Add(receptionistMember);

        var receptionistPermissions = DefaultPermissions.ForRole(ClinicMemberRole.Receptionist)
            .Select(p => new ClinicMemberPermission { ClinicMemberId = receptionistMember.Id, Permission = p });
        _context.Set<ClinicMemberPermission>().AddRange(receptionistPermissions);

        await _context.SaveChangesAsync();
    }

    private void LogError(string role, IdentityResult result) =>
        _logger.LogError("Failed to create {Role}: {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Description)));

    // ── Appointment seed data ─────────────────────────────────────────────────

    private async Task SeedAppointmentDataAsync(Clinic clinic)
    {
        // Skip if appointments already exist for this clinic
        var hasAppointments = await _context.Set<Appointment>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AnyAsync(a => a.ClinicId == clinic.Id);
        if (hasAppointments) return;

        var branch = await _context.Set<ClinicBranch>()
            .FirstOrDefaultAsync(b => b.ClinicId == clinic.Id && b.IsMainBranch);
        if (branch is null) return;

        // Get the two doctors (owner + seeded doctor)
        var ownerDoctorInfo = await _context.Set<DoctorInfo>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Include(d => d.ClinicMember)
            .FirstOrDefaultAsync(d => d.ClinicMember.ClinicId == clinic.Id && d.ClinicMember.Role == ClinicMemberRole.Owner);

        var staffDoctorInfo = await _context.Set<DoctorInfo>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Include(d => d.ClinicMember)
            .FirstOrDefaultAsync(d => d.ClinicMember.ClinicId == clinic.Id && d.ClinicMember.Role == ClinicMemberRole.Doctor);

        if (ownerDoctorInfo is null || staffDoctorInfo is null) return;

        // Set appointment types
        ownerDoctorInfo.AppointmentType = AppointmentType.Queue;
        staffDoctorInfo.AppointmentType = AppointmentType.Time;

        // ── Second branch ─────────────────────────────────────────────────────

        var secondBranch = new ClinicBranch
        {
            ClinicId       = clinic.Id,
            Name           = "West Branch",
            AddressLine    = "456 Health Avenue, West District",
            StateGeonameId = 360630,
            CityGeonameId  = 360630,
            IsMainBranch   = false,
            IsActive       = true,
        };
        _context.Set<ClinicBranch>().Add(secondBranch);

        // ── Working days ──────────────────────────────────────────────────────

        // Owner doctor: Queue — works Sun–Thu (0,1,2,3,4)
        var ownerSchedule = new DoctorBranchSchedule { DoctorInfoId = ownerDoctorInfo.Id, BranchId = branch.Id, IsActive = true };
        _context.Set<DoctorBranchSchedule>().Add(ownerSchedule);
        foreach (var day in new[] { 0, 1, 2, 3, 4 })
            _context.Set<WorkingDay>().Add(new WorkingDay
            {
                DoctorBranchScheduleId = ownerSchedule.Id,
                Day = (DayOfWeek)day, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0), IsAvailable = true,
            });

        // Staff doctor: Time — works Mon–Fri (1,2,3,4,5)
        var staffSchedule = new DoctorBranchSchedule { DoctorInfoId = staffDoctorInfo.Id, BranchId = branch.Id, IsActive = true };
        _context.Set<DoctorBranchSchedule>().Add(staffSchedule);
        foreach (var day in new[] { 1, 2, 3, 4, 5 })
            _context.Set<WorkingDay>().Add(new WorkingDay
            {
                DoctorBranchScheduleId = staffSchedule.Id,
                Day = (DayOfWeek)day, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0), IsAvailable = true,
            });

        // ── Visit types ───────────────────────────────────────────────────────

        var ownerVt1 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id, NameEn = "Consultation", NameAr = "كشف",     Price = 150, IsActive = true };
        var ownerVt2 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id, NameEn = "Follow-up",    NameAr = "متابعة",  Price = 80,  IsActive = true };
        var ownerVt3 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id, NameEn = "Emergency",    NameAr = "طوارئ",   Price = 250, IsActive = true };
        var staffVt1 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id, NameEn = "Consultation", NameAr = "كشف",     Price = 200, IsActive = true };
        var staffVt2 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id, NameEn = "ECG",          NameAr = "رسم قلب", Price = 120, IsActive = true };
        var staffVt3 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id, NameEn = "Echo",         NameAr = "إيكو",    Price = 300, IsActive = true };
        _context.Set<VisitType>().AddRange(ownerVt1, ownerVt2, ownerVt3, staffVt1, staffVt2, staffVt3);

        // ── 20 Patients ───────────────────────────────────────────────────────

        var patientData = new[]
        {
            ("Ahmed Hassan",       "Male",   "1985-03-15", "+201001234567", "A+"),
            ("Fatima Al-Zahra",    "Female", "1990-07-22", "+201112345678", "B+"),
            ("Mohamed Ali",        "Male",   "1978-11-08", "+201223456789", "O+"),
            ("Sara Ibrahim",       "Female", "1995-02-14", "+201334567890", "AB+"),
            ("Khaled Mahmoud",     "Male",   "1982-09-30", "+201445678901", "A-"),
            ("Nour El-Din",        "Female", "1998-05-18", "+201556789012", "B-"),
            ("Omar Sherif",        "Male",   "1975-12-03", "+201667890123", "O-"),
            ("Layla Hassan",       "Female", "1992-08-25", "+201778901234", "AB-"),
            ("Youssef Kamal",      "Male",   "1988-04-10", "+201889012345", "A+"),
            ("Hana Mostafa",       "Female", "1993-06-28", "+201990123456", "B+"),
            ("Tarek Nasser",       "Male",   "1970-01-17", "+201001122334", "O+"),
            ("Rania Fawzy",        "Female", "1987-10-05", "+201112233445", "A+"),
            ("Amr Saad",           "Male",   "1996-03-22", "+201223344556", "AB+"),
            ("Dina Khalil",        "Female", "1983-08-14", "+201334455667", "B+"),
            ("Hassan Ramadan",     "Male",   "1979-12-30", "+201445566778", "O+"),
            ("Mona Adel",          "Female", "1991-05-07", "+201556677889", "A-"),
            ("Sherif Gamal",       "Male",   "1986-09-19", "+201667788990", "B+"),
            ("Aya Mahmoud",        "Female", "1994-02-28", "+201778899001", "O+"),
            ("Karim Farouk",       "Male",   "1977-07-11", "+201889900112", "A+"),
            ("Noha Sayed",         "Female", "1989-11-23", "+201990011223", "AB+"),
        };

        var patients = new List<Patient>();
        for (var i = 0; i < patientData.Length; i++)
        {
            var (name, gender, dob, phone, blood) = patientData[i];
            var person = new Person
            {
                FullName    = name,
                Gender      = gender == "Male" ? Gender.Male : Gender.Female,
                DateOfBirth = DateOnly.Parse(dob),
            };
            var patient = new Patient
            {
                ClinicId    = clinic.Id,
                PatientCode = (i + 1).ToString("D4"),
                BloodType   = blood switch {
                    "A+"  => BloodType.APositive,  "A-"  => BloodType.ANegative,
                    "B+"  => BloodType.BPositive,  "B-"  => BloodType.BNegative,
                    "AB+" => BloodType.ABPositive, "AB-" => BloodType.ABNegative,
                    "O+"  => BloodType.OPositive,  "O-"  => BloodType.ONegative,
                    _ => (BloodType?)null
                },
                PersonId    = person.Id,
                Person      = person,
                CreatedAt   = DateTimeOffset.UtcNow.AddDays(-(patientData.Length - i) * 3),
            };
            _context.Set<Person>().Add(person);
            _context.Set<Patient>().Add(patient);
            patients.Add(patient);
            _context.Set<PatientPhone>().Add(new PatientPhone { PatientId = patient.Id, PhoneNumber = phone, NationalNumber = phone });
        }

        await _context.SaveChangesAsync();

        // ── Appointments ──────────────────────────────────────────────────────

        var today    = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        // ── Owner (Queue) — today: 8 appointments ─────────────────────────────
        var queueStatuses = new[] {
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.InProgress, AppointmentStatus.Pending, AppointmentStatus.Pending,
            AppointmentStatus.Pending, AppointmentStatus.Pending,
        };
        for (var i = 0; i < 8; i++)
        {
            var vt = i % 3 == 0 ? ownerVt1 : i % 3 == 1 ? ownerVt2 : ownerVt3;
            var appt = new Appointment
            {
                ClinicId = clinic.Id, BranchId = branch.Id,
                PatientId = patients[i].Id, DoctorInfoId = ownerDoctorInfo.Id,
                VisitTypeId = vt.Id, Date = today,
                Type = AppointmentType.Queue, QueueNumber = i + 1,
                Status = queueStatuses[i],
            };
            appt.ApplyPrice(vt.Price, i == 3 ? 10m : null);
            _context.Set<Appointment>().Add(appt);
        }

        // ── Owner (Queue) — yesterday: 5 completed ───────────────────────────
        for (var i = 0; i < 5; i++)
        {
            var vt = i % 2 == 0 ? ownerVt1 : ownerVt2;
            var appt = new Appointment
            {
                ClinicId = clinic.Id, BranchId = branch.Id,
                PatientId = patients[i + 8].Id, DoctorInfoId = ownerDoctorInfo.Id,
                VisitTypeId = vt.Id, Date = yesterday,
                Type = AppointmentType.Queue, QueueNumber = i + 1,
                Status = AppointmentStatus.Completed,
            };
            appt.ApplyPrice(vt.Price);
            _context.Set<Appointment>().Add(appt);
        }

        // ── Owner (Queue) — tomorrow: 4 pending ──────────────────────────────
        for (var i = 0; i < 4; i++)
        {
            var vt = i % 2 == 0 ? ownerVt1 : ownerVt2;
            var appt = new Appointment
            {
                ClinicId = clinic.Id, BranchId = branch.Id,
                PatientId = patients[i + 4].Id, DoctorInfoId = ownerDoctorInfo.Id,
                VisitTypeId = vt.Id, Date = tomorrow,
                Type = AppointmentType.Queue, QueueNumber = i + 1,
                Status = AppointmentStatus.Pending,
            };
            appt.ApplyPrice(vt.Price);
            _context.Set<Appointment>().Add(appt);
        }

        // ── Staff (Time) — today: 6 appointments ─────────────────────────────
        var timeTimes = new[] { "09:00", "09:30", "10:00", "10:30", "11:00", "11:30" };
        var timeStatuses = new[] {
            AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.InProgress, AppointmentStatus.Pending,
            AppointmentStatus.Pending, AppointmentStatus.Pending,
        };
        for (var i = 0; i < 6; i++)
        {
            var vt = i % 3 == 0 ? staffVt1 : i % 3 == 1 ? staffVt2 : staffVt3;
            var appt = new Appointment
            {
                ClinicId = clinic.Id, BranchId = branch.Id,
                PatientId = patients[i + 10].Id, DoctorInfoId = staffDoctorInfo.Id,
                VisitTypeId = vt.Id, Date = today,
                Type = AppointmentType.Time, ScheduledTime = TimeOnly.Parse(timeTimes[i]),
                Status = timeStatuses[i],
            };
            appt.ApplyPrice(vt.Price);
            _context.Set<Appointment>().Add(appt);
        }

        // ── Staff (Time) — tomorrow: 4 pending ───────────────────────────────
        var tomorrowTimes = new[] { "09:00", "10:00", "11:00", "14:00" };
        for (var i = 0; i < 4; i++)
        {
            var vt = i % 2 == 0 ? staffVt1 : staffVt2;
            var appt = new Appointment
            {
                ClinicId = clinic.Id, BranchId = branch.Id,
                PatientId = patients[i + 2].Id, DoctorInfoId = staffDoctorInfo.Id,
                VisitTypeId = vt.Id, Date = tomorrow,
                Type = AppointmentType.Time, ScheduledTime = TimeOnly.Parse(tomorrowTimes[i]),
                Status = AppointmentStatus.Pending,
            };
            appt.ApplyPrice(vt.Price);
            _context.Set<Appointment>().Add(appt);
        }

        // ── QueueCounters ─────────────────────────────────────────────────────
        _context.Set<QueueCounter>().Add(new QueueCounter { DoctorInfoId = ownerDoctorInfo.Id, Date = today,     LastValue = 8 });
        _context.Set<QueueCounter>().Add(new QueueCounter { DoctorInfoId = ownerDoctorInfo.Id, Date = yesterday, LastValue = 5 });
        _context.Set<QueueCounter>().Add(new QueueCounter { DoctorInfoId = ownerDoctorInfo.Id, Date = tomorrow,  LastValue = 4 });

        // ── Testimonials ──────────────────────────────────────────────────────
        var ownerUser = await _userManager.FindByEmailAsync(_options.ClinicOwner.Email);
        if (ownerUser is not null)
        {
            _context.Set<Testimonial>().Add(new Testimonial
            {
                ClinicId   = clinic.Id,
                UserId     = ownerUser.Id,
                ClinicName = clinic.Name,
                AuthorName = "Dr. John Smith",
                Position   = "General Practitioner",
                Text       = "ClinicCare has transformed how we manage our clinic. The appointment system is intuitive and our patients love the seamless experience.",
                Rating     = 5,
                IsApproved = true,
            });
        }

        // ── Contact messages ──────────────────────────────────────────────────
        _context.Set<ContactMessage>().AddRange(
            new ContactMessage { FirstName = "Sherif", LastName = "Ali", Email = "sherif@example.com", Subject = "Pricing inquiry", Message = "I want to know more about the Growing Clinic plan and what features are included.", CreatedAt = DateTimeOffset.UtcNow.AddDays(-2) },
            new ContactMessage { FirstName = "Mona",   LastName = "Kamal", Email = "mona@example.com", Subject = "Technical support", Message = "I'm having trouble logging in with my Google account. Can you help?", CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) },
            new ContactMessage { FirstName = "Tarek",  LastName = "Nour", Email = "tarek@example.com", Subject = "Feature request", Message = "Would love to see a mobile app version of the dashboard for doctors on the go.", CreatedAt = DateTimeOffset.UtcNow.AddHours(-3) }
        );

        await _context.SaveChangesAsync();
        _logger.LogInformation("Full seed data created for clinic {Id} — {Count} patients, appointments, testimonials, contact messages", clinic.Id, patients.Count);
    }
}
