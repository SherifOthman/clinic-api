using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds appointments covering all scenarios:
/// - Queue appointments: today (mixed statuses), yesterday (completed), tomorrow (pending)
/// - Time appointments: today (mixed statuses), tomorrow (pending), next week
/// - Waiting status scenario
/// - Cancelled and NoShow scenarios
/// - Appointments with discounts
/// - QueueCounters for all seeded days
/// </summary>
public class DemoAppointmentSeed
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoAppointmentSeed> _logger;

    public DemoAppointmentSeed(ApplicationDbContext db, ILogger<DemoAppointmentSeed> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(
        Guid clinicId,
        ClinicBranch branch,
        DoctorInfo ownerDoctor,
        DoctorInfo staffDoctor,
        VisitType ownerVt1, VisitType ownerVt2, VisitType ownerVt3,
        VisitType staffVt1, VisitType staffVt2, VisitType staffVt3,
        List<Patient> patients)
    {
        var today     = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);
        var tomorrow  = today.AddDays(1);
        var nextWeek  = today.AddDays(7);
        var lastWeek  = today.AddDays(-7);

        // ── QUEUE appointments (owner doctor) ─────────────────────────────────

        // Today — 10 appointments: 3 completed, 1 in-progress, 1 waiting, 5 pending
        var queueToday = new[]
        {
            (patients[0],  ownerVt1, AppointmentStatus.Completed,  1,  null as decimal?),
            (patients[1],  ownerVt2, AppointmentStatus.Completed,  2,  null),
            (patients[2],  ownerVt1, AppointmentStatus.Completed,  3,  10m),   // 10% discount
            (patients[3],  ownerVt3, AppointmentStatus.InProgress, 4,  null),
            (patients[4],  ownerVt1, AppointmentStatus.Waiting,    5,  null),  // patient arrived
            (patients[5],  ownerVt2, AppointmentStatus.Pending,    6,  null),
            (patients[6],  ownerVt1, AppointmentStatus.Pending,    7,  null),
            (patients[7],  ownerVt2, AppointmentStatus.Pending,    8,  null),
            (patients[8],  ownerVt1, AppointmentStatus.Pending,    9,  null),
            (patients[9],  ownerVt3, AppointmentStatus.Pending,    10, 20m),   // 20% discount
        };
        foreach (var (p, vt, status, q, disc) in queueToday)
            AddQueueAppt(clinicId, branch.Id, p.Id, ownerDoctor.Id, vt, today, q, status, disc);

        // Yesterday — 8 completed
        for (var i = 0; i < 8; i++)
        {
            var vt = i % 3 == 0 ? ownerVt1 : i % 3 == 1 ? ownerVt2 : ownerVt3;
            AddQueueAppt(clinicId, branch.Id, patients[i + 10].Id, ownerDoctor.Id, vt, yesterday, i + 1, AppointmentStatus.Completed, null);
        }

        // Day before yesterday — 6 completed, 1 no-show, 1 cancelled
        var twoDaysAgo = today.AddDays(-2);
        for (var i = 0; i < 6; i++)
            AddQueueAppt(clinicId, branch.Id, patients[i].Id, ownerDoctor.Id, ownerVt1, twoDaysAgo, i + 1, AppointmentStatus.Completed, null);
        AddQueueAppt(clinicId, branch.Id, patients[6].Id, ownerDoctor.Id, ownerVt2, twoDaysAgo, 7, AppointmentStatus.NoShow, null);
        AddQueueAppt(clinicId, branch.Id, patients[7].Id, ownerDoctor.Id, ownerVt3, twoDaysAgo, 8, AppointmentStatus.Cancelled, null);

        // Last week — 5 completed
        for (var i = 0; i < 5; i++)
            AddQueueAppt(clinicId, branch.Id, patients[i + 5].Id, ownerDoctor.Id, ownerVt1, lastWeek, i + 1, AppointmentStatus.Completed, null);

        // Tomorrow — 6 pending
        for (var i = 0; i < 6; i++)
        {
            var vt = i % 2 == 0 ? ownerVt1 : ownerVt2;
            AddQueueAppt(clinicId, branch.Id, patients[i + 3].Id, ownerDoctor.Id, vt, tomorrow, i + 1, AppointmentStatus.Pending, null);
        }

        // Next week — 4 pending
        for (var i = 0; i < 4; i++)
            AddQueueAppt(clinicId, branch.Id, patients[i + 8].Id, ownerDoctor.Id, ownerVt1, nextWeek, i + 1, AppointmentStatus.Pending, null);

        // ── TIME appointments (staff doctor) ──────────────────────────────────

        // Today — 8 appointments with various statuses
        var timeSlotsToday = new[]
        {
            ("08:00", patients[0],  staffVt1, AppointmentStatus.Completed,  30, null as decimal?),
            ("08:30", patients[1],  staffVt2, AppointmentStatus.Completed,  30, null),
            ("09:00", patients[2],  staffVt3, AppointmentStatus.Completed,  45, 15m),
            ("09:45", patients[3],  staffVt1, AppointmentStatus.InProgress, 30, null),
            ("10:15", patients[4],  staffVt2, AppointmentStatus.Waiting,    30, null),
            ("10:45", patients[5],  staffVt1, AppointmentStatus.Pending,    30, null),
            ("11:15", patients[6],  staffVt3, AppointmentStatus.Pending,    45, null),
            ("12:00", patients[7],  staffVt1, AppointmentStatus.Pending,    30, 10m),
        };
        foreach (var (time, p, vt, status, dur, disc) in timeSlotsToday)
            AddTimeAppt(clinicId, branch.Id, p.Id, staffDoctor.Id, vt, today, time, dur, status, disc);

        // Yesterday — 6 completed, 1 no-show, 1 cancelled
        var timeSlotsYesterday = new[]
        {
            ("09:00", patients[10], staffVt1, AppointmentStatus.Completed,  30),
            ("09:30", patients[11], staffVt2, AppointmentStatus.Completed,  30),
            ("10:00", patients[12], staffVt1, AppointmentStatus.Completed,  30),
            ("10:30", patients[13], staffVt3, AppointmentStatus.Completed,  45),
            ("11:15", patients[14], staffVt1, AppointmentStatus.Completed,  30),
            ("11:45", patients[15], staffVt2, AppointmentStatus.Completed,  30),
            ("13:00", patients[16], staffVt1, AppointmentStatus.NoShow,     30),
            ("13:30", patients[17], staffVt3, AppointmentStatus.Cancelled,  45),
        };
        foreach (var (time, p, vt, status, dur) in timeSlotsYesterday)
            AddTimeAppt(clinicId, branch.Id, p.Id, staffDoctor.Id, vt, yesterday, time, dur, status, null);

        // Tomorrow — 6 pending
        var timeSlotsTomorrow = new[]
        {
            ("08:00", patients[0],  staffVt1, 30),
            ("08:30", patients[1],  staffVt2, 30),
            ("09:00", patients[2],  staffVt3, 45),
            ("09:45", patients[3],  staffVt1, 30),
            ("10:15", patients[4],  staffVt2, 30),
            ("11:00", patients[5],  staffVt1, 30),
        };
        foreach (var (time, p, vt, dur) in timeSlotsTomorrow)
            AddTimeAppt(clinicId, branch.Id, p.Id, staffDoctor.Id, vt, tomorrow, time, dur, AppointmentStatus.Pending, null);

        // Next week — 4 pending
        var timeSlotsNextWeek = new[]
        {
            ("09:00", patients[6],  staffVt1, 30),
            ("09:30", patients[7],  staffVt3, 45),
            ("10:15", patients[8],  staffVt2, 30),
            ("11:00", patients[9],  staffVt1, 30),
        };
        foreach (var (time, p, vt, dur) in timeSlotsNextWeek)
            AddTimeAppt(clinicId, branch.Id, p.Id, staffDoctor.Id, vt, nextWeek, time, dur, AppointmentStatus.Pending, null);

        // ── QueueCounters ─────────────────────────────────────────────────────
        _db.Set<QueueCounter>().AddRange(
            new QueueCounter { DoctorInfoId = ownerDoctor.Id, Date = today,     LastValue = 10 },
            new QueueCounter { DoctorInfoId = ownerDoctor.Id, Date = yesterday, LastValue = 8  },
            new QueueCounter { DoctorInfoId = ownerDoctor.Id, Date = twoDaysAgo,LastValue = 8  },
            new QueueCounter { DoctorInfoId = ownerDoctor.Id, Date = lastWeek,  LastValue = 5  },
            new QueueCounter { DoctorInfoId = ownerDoctor.Id, Date = tomorrow,  LastValue = 6  },
            new QueueCounter { DoctorInfoId = ownerDoctor.Id, Date = nextWeek,  LastValue = 4  }
        );

        await _db.SaveChangesAsync();
        _logger.LogInformation("Demo appointments seeded");
    }

    private void AddQueueAppt(Guid clinicId, Guid branchId, Guid patientId, Guid doctorId,
        VisitType vt, DateOnly date, int queueNum, AppointmentStatus status, decimal? discount)
    {
        var appt = new Appointment
        {
            ClinicId     = clinicId,
            BranchId     = branchId,
            PatientId    = patientId,
            DoctorInfoId = doctorId,
            VisitTypeId  = vt.Id,
            Date         = date,
            Type         = AppointmentType.Queue,
            QueueNumber  = queueNum,
            Status       = status,
        };
        appt.ApplyPrice(vt.Price, discount);
        _db.Set<Appointment>().Add(appt);
    }

    private void AddTimeAppt(Guid clinicId, Guid branchId, Guid patientId, Guid doctorId,
        VisitType vt, DateOnly date, string time, int durationMin,
        AppointmentStatus status, decimal? discount)
    {
        var start = TimeOnly.Parse(time);
        var appt = new Appointment
        {
            ClinicId             = clinicId,
            BranchId             = branchId,
            PatientId            = patientId,
            DoctorInfoId         = doctorId,
            VisitTypeId          = vt.Id,
            Date                 = date,
            Type                 = AppointmentType.Time,
            ScheduledTime        = start,
            EndTime              = start.AddMinutes(durationMin),
            VisitDurationMinutes = durationMin,
            Status               = status,
        };
        appt.ApplyPrice(vt.Price, discount);
        _db.Set<Appointment>().Add(appt);
    }
}
