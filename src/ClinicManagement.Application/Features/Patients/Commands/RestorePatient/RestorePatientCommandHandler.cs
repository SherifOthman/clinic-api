using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class RestorePatientCommandHandler : IRequestHandler<RestorePatientCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestorePatientCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RestorePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .IgnoreQueryFilters([QueryFilterNames.SoftDelete, QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (patient is null)
            return Result.Failure(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");

        if (!patient.IsDeleted)
            return Result.Failure(ErrorCodes.PATIENT_NOT_DELETED, "Patient is not deleted");

        patient.IsDeleted = false;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
