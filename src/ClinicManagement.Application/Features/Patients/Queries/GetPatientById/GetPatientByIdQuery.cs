using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<Result<PatientDto>>;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.ClinicId!.Value;

        var patient = await _context.Patients
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
                .ThenInclude(pcd => pcd.ChronicDisease)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.ClinicId == clinicId, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDto>.Fail(MessageCodes.Patient.NOT_FOUND);
        }

        var patientDto = patient.Adapt<PatientDto>();
        return Result<PatientDto>.Ok(patientDto);
    }
}
