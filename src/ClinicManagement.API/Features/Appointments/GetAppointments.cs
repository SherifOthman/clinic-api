using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Appointments;

public class GetAppointmentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/appointments", HandleAsync)
            .RequireAuthorization()
            .WithName("GetAppointments")
            .WithSummary("Get appointments with filtering and pagination")
            .WithTags("Appointments")
            .Produces<PaginatedResult<AppointmentListItem>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // Build query with filters - ClinicId filter is automatic via global query filter
        var query = db.Appointments.AsQueryable();

        // Apply filters using WhereIf
        if (request.Date.HasValue)
        {
            var date = request.Date.Value.Date;
            query = query.Where(a => a.AppointmentDate == date);
        }

        query = query
            .WhereIf(request.DoctorId.HasValue, a => a.DoctorId == request.DoctorId!.Value)
            .WhereIf(request.PatientId.HasValue, a => a.PatientId == request.PatientId!.Value)
            .WhereIf(request.AppointmentTypeId.HasValue, a => a.AppointmentTypeId == request.AppointmentTypeId!.Value)
            .WhereIf(request.Status.HasValue, a => a.Status == request.Status!.Value);

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination and sorting
        var items = await query
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.QueueNumber)
            .Paginate(request.Page, request.PageSize)
            .Select(a => new AppointmentListItem(
                a.Id,
                a.Patient.FullName,
                a.Patient.PatientCode,
                a.DoctorProfile.Staff.User.FirstName + " " + a.DoctorProfile.Staff.User.LastName,
                a.ClinicBranch.Name,
                a.AppointmentType.NameEn,
                a.AppointmentDate,
                a.QueueNumber,
                a.Status
            ))
            .ToListAsync(ct);

        return Results.Ok(new PaginatedResult<AppointmentListItem>(
            items,
            totalCount,
            request.Page,
            request.PageSize
        ));
    }

    public record Request(
        DateTime? Date = null,
        Guid? DoctorId = null,
        Guid? PatientId = null,
        Guid? AppointmentTypeId = null,
        AppointmentStatus? Status = null,
        int Page = 1,
        int PageSize = 20);

    public record AppointmentListItem(
        Guid Id,
        string PatientName,
        string PatientCode,
        string DoctorName,
        string BranchName,
        string AppointmentTypeName,
        DateTime AppointmentDate,
        short QueueNumber,
        AppointmentStatus Status);
}
