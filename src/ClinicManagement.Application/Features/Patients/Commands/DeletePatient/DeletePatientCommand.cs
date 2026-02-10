using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.DeletePatient;

public record DeletePatientCommand(Guid Id) : IRequest<Result<bool>>;

public class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeletePatientCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.ClinicId!.Value;

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.ClinicId == clinicId, cancellationToken);

        if (patient == null)
        {
            return Result<bool>.Fail(MessageCodes.Patient.NOT_FOUND);
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
