using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Medicines;

public class GetMedicinesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/medicines", HandleAsync)
            .RequireAuthorization("InventoryManagement")
            .WithName("GetMedicines")
            .WithSummary("List medicines with pagination and filtering")
            .WithTags("Medicines")
            .Produces<PaginatedResult<MedicineListItem>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        DateTimeProvider dateTimeProvider,
        CancellationToken ct)
    {
        // Build query - ClinicId filter is automatic via global query filter
        var query = db.Medicines
            .WhereIf(request.ClinicBranchId.HasValue, m => m.ClinicBranchId == request.ClinicBranchId!.Value)
            .WhereIf(request.IsActive.HasValue, m => m.IsActive == request.IsActive!.Value)
            .WhereIf(request.IsLowStock == true, m => m.TotalStripsInStock <= m.MinimumStockLevel)
            .WhereIf(request.IsExpired == true, m => m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date < dateTimeProvider.UtcNow.Date);

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination and sorting
        var items = await query
            .OrderBy(m => m.Name)
            .Paginate(request.Page, request.PageSize)
            .Select(m => new MedicineListItem(
                m.Id,
                m.Name,
                m.Manufacturer,
                m.BoxPrice,
                m.StripPrice,
                m.StripsPerBox,
                m.TotalStripsInStock,
                m.FullBoxesInStock,
                m.RemainingStrips,
                m.StockStatus,
                m.IsActive,
                m.IsDiscontinued,
                m.IsExpired,
                m.ExpiryDate
            ))
            .ToListAsync(ct);

        return Results.Ok(new PaginatedResult<MedicineListItem>(
            items,
            totalCount,
            request.Page,
            request.PageSize
        ));
    }

    public record Request(
        Guid? ClinicBranchId = null,
        bool? IsActive = null,
        bool? IsLowStock = null,
        bool? IsExpired = null,
        int Page = 1,
        int PageSize = 20);

    public record MedicineListItem(
        Guid Id,
        string Name,
        string? Manufacturer,
        decimal BoxPrice,
        decimal StripPrice,
        int StripsPerBox,
        int TotalStripsInStock,
        int FullBoxesInStock,
        int RemainingStrips,
        StockStatus StockStatus,
        bool IsActive,
        bool IsDiscontinued,
        bool IsExpired,
        DateTime? ExpiryDate);
}
