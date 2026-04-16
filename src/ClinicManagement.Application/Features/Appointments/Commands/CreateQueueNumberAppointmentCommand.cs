using ClinicManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public record CreateQueueNumberAppointmentCommand(
    Guid ClinicBranchId,
    Guid PatientId,
    Guid DoctorId,
    DateOnly Date,
    Guid DoctorVisitTypeId,
    decimal? DiscountPercent);
