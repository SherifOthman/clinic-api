using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Features.Patients;

public class GetPatientsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients", HandleAsync)
            .RequireAuthorization("StaffAccess")
            .WithName("GetPatients")
            .WithSummary("Get patients with filtering, sorting, and pagination")
            .WithTags("Patients")
            .Produces<PaginatedResult<PatientSummary>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // Build base query - ClinicId filter is automatic via global query filter
        var query = db.Patients
            .SearchBy(request.SearchTerm, p => p.FullName, p => p.PatientCode)
            .WhereIf(request.IsMale.HasValue, p => p.IsMale == request.IsMale!.Value)
            .WhereIf(request.DateOfBirthFrom.HasValue, p => p.DateOfBirth >= request.DateOfBirthFrom!.Value)
            .WhereIf(request.DateOfBirthTo.HasValue, p => p.DateOfBirth <= request.DateOfBirthTo!.Value);

        // Apply age filters
        if (request.MinAge.HasValue)
        {
            var maxDob = DateTimeExtensions.GetMaxDateOfBirthForMinAge(request.MinAge.Value);
            query = query.Where(p => p.DateOfBirth <= maxDob);
        }

        if (request.MaxAge.HasValue)
        {
            var minDob = DateTimeExtensions.GetMinDateOfBirthForMaxAge(request.MaxAge.Value);
            query = query.Where(p => p.DateOfBirth >= minDob);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "asc"
                ? query.OrderBy(p => p.FullName)
                : query.OrderByDescending(p => p.FullName),
            "code" => request.SortDirection == "asc"
                ? query.OrderBy(p => p.PatientCode)
                : query.OrderByDescending(p => p.PatientCode),
            "age" => request.SortDirection == "asc"
                ? query.OrderBy(p => p.DateOfBirth)
                : query.OrderByDescending(p => p.DateOfBirth),
            "date" or "created" => request.SortDirection == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // Apply pagination and project to DTO
        var patients = await query
            .Paginate(request.PageNumber, request.PageSize)
            .Select(p => new PatientSummary(
                p.Id,
                p.PatientCode,
                p.FullName,
                p.IsMale,
                p.DateOfBirth,
                p.DateOfBirth.CalculateAge(),
                p.PhoneNumbers
                    .Where(pn => pn.IsPrimary)
                    .Select(pn => pn.PhoneNumber)
                    .FirstOrDefault(),
                p.ChronicDiseases.Count,
                p.CreatedAt
            ))
            .ToListAsync(ct);

        return Results.Ok(new PaginatedResult<PatientSummary>(
            patients,
            totalCount,
            request.PageNumber,
            request.PageSize
        ));
    }

    public record Request(
        string? SearchTerm = null,
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        string SortDirection = "desc",
        bool? IsMale = null,
        DateTime? DateOfBirthFrom = null,
        DateTime? DateOfBirthTo = null,
        int? MinAge = null,
        int? MaxAge = null);

    public record PatientSummary(
        Guid Id,
        string PatientCode,
        string FullName,
        bool IsMale,
        DateTime DateOfBirth,
        int Age,
        string? PrimaryPhone,
        int ChronicDiseaseCount,
        DateTime CreatedAt);
}
