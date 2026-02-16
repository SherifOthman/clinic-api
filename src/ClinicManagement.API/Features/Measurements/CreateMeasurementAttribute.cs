using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Measurements;

public class CreateMeasurementAttributeEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/measurement-attributes", HandleAsync)
            .RequireAuthorization()
            .WithName("CreateMeasurementAttribute")
            .WithSummary("Create a new measurement attribute")
            .WithTags("Measurements")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        // Check for duplicate name
        var duplicateExists = await db.MeasurementAttributes
            .AnyAsync(ma =>
                ma.NameEn == request.NameEn || ma.NameAr == request.NameAr, ct);

        if (duplicateExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.DUPLICATE_ATTRIBUTE,
                Title = "Duplicate Attribute",
                Status = StatusCodes.Status400BadRequest,
                Detail = "A measurement attribute with this name already exists"
            });

        var attribute = new MeasurementAttribute
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            DataType = ClinicManagement.API.Common.Enums.MeasurementDataType.Text
        };

        db.MeasurementAttributes.Add(attribute);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/measurement-attributes/{attribute.Id}", new Response(attribute.Id));
    }

    public record Request(
        [Required]
        [MaxLength(80)]
        string NameEn,
        
        [Required]
        [MaxLength(80)]
        string NameAr,
        
        [MaxLength(255)]
        string? DescriptionEn,
        
        [MaxLength(255)]
        string? DescriptionAr);

    public record Response(Guid Id);
}
