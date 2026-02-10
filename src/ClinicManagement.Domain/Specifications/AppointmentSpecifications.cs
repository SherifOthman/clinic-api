using System;
using System.Linq.Expressions;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Specifications;

/// <summary>
/// Specification for appointments on a specific date
/// </summary>
public class AppointmentOnDateSpecification : Specification<Appointment>
{
    private readonly DateTime _date;

    public AppointmentOnDateSpecification(DateTime date)
    {
        _date = date.Date;
    }

    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var date = _date;
        return appointment => appointment.AppointmentDate.Date == date;
    }
}

/// <summary>
/// Specification for appointments in date range
/// </summary>
public class AppointmentInDateRangeSpecification : Specification<Appointment>
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public AppointmentInDateRangeSpecification(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate.Date;
        _endDate = endDate.Date;
    }

    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var startDate = _startDate;
        var endDate = _endDate;
        return appointment => appointment.AppointmentDate.Date >= startDate 
            && appointment.AppointmentDate.Date <= endDate;
    }
}

/// <summary>
/// Specification for appointments by status
/// </summary>
public class AppointmentByStatusSpecification : Specification<Appointment>
{
    private readonly AppointmentStatus _status;

    public AppointmentByStatusSpecification(AppointmentStatus status)
    {
        _status = status;
    }

    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var status = _status;
        return appointment => appointment.Status == status;
    }
}

/// <summary>
/// Specification for pending appointments
/// </summary>
public class PendingAppointmentSpecification : Specification<Appointment>
{
    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        return appointment => appointment.Status == AppointmentStatus.Pending;
    }
}

/// <summary>
/// Specification for confirmed appointments
/// </summary>
public class ConfirmedAppointmentSpecification : Specification<Appointment>
{
    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        return appointment => appointment.Status == AppointmentStatus.Confirmed;
    }
}

/// <summary>
/// Specification for appointments by doctor
/// </summary>
public class AppointmentByDoctorSpecification : Specification<Appointment>
{
    private readonly Guid _doctorId;

    public AppointmentByDoctorSpecification(Guid doctorId)
    {
        _doctorId = doctorId;
    }

    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var doctorId = _doctorId;
        return appointment => appointment.DoctorId == doctorId;
    }
}

/// <summary>
/// Specification for appointments by patient
/// </summary>
public class AppointmentByPatientSpecification : Specification<Appointment>
{
    private readonly Guid _patientId;

    public AppointmentByPatientSpecification(Guid patientId)
    {
        _patientId = patientId;
    }

    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var patientId = _patientId;
        return appointment => appointment.PatientId == patientId;
    }
}

/// <summary>
/// Specification for appointments by clinic branch
/// </summary>
public class AppointmentByClinicBranchSpecification : Specification<Appointment>
{
    private readonly Guid _clinicBranchId;

    public AppointmentByClinicBranchSpecification(Guid clinicBranchId)
    {
        _clinicBranchId = clinicBranchId;
    }

    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var clinicBranchId = _clinicBranchId;
        return appointment => appointment.ClinicBranchId == clinicBranchId;
    }
}

/// <summary>
/// Specification for today's appointments
/// </summary>
public class TodayAppointmentSpecification : Specification<Appointment>
{
    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var today = DateTime.UtcNow.Date;
        return appointment => appointment.AppointmentDate.Date == today;
    }
}

/// <summary>
/// Specification for upcoming appointments (future dates)
/// </summary>
public class UpcomingAppointmentSpecification : Specification<Appointment>
{
    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var now = DateTime.UtcNow;
        return appointment => appointment.AppointmentDate > now 
            && appointment.Status != AppointmentStatus.Cancelled;
    }
}

/// <summary>
/// Specification for past appointments
/// </summary>
public class PastAppointmentSpecification : Specification<Appointment>
{
    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        var now = DateTime.UtcNow;
        return appointment => appointment.AppointmentDate < now;
    }
}

/// <summary>
/// Specification for unpaid appointments (consultation fee not paid)
/// </summary>
public class UnpaidAppointmentSpecification : Specification<Appointment>
{
    public override Expression<Func<Appointment, bool>> ToExpression()
    {
        return appointment => !appointment.IsConsultationFeePaid 
            && appointment.Status != AppointmentStatus.Cancelled;
    }
}
