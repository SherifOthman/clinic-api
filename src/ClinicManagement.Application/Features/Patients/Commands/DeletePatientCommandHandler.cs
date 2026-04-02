using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeletePatientCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeletePatientCommand request,
        CancellationToken cancellationToken)
    {
        // ClinicId filter applied automatically via global named filter
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (patient == null)
        {
            return Result.Failure(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");
        }

        // Soft delete
        patient.SoftDelete();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
