using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Clinics.Commands.UpdateClinic;

public class UpdateClinicCommandHandler : IRequestHandler<UpdateClinicCommand, Result<ClinicDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateClinicCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(UpdateClinicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _context.UnitOfWork.Clinics.GetByIdAsync(request.Id, cancellationToken);
            if (clinic == null)
                return Result<ClinicDto>.Failure("Clinic not found");

            clinic.Name = request.Name;
            clinic.Phone = request.Phone;
            clinic.UpdatedAt = DateTime.UtcNow;

            _context.UnitOfWork.Clinics.Update(clinic);
            await _context.SaveChangesAsync(cancellationToken);

            var clinicDto = _mapper.Map<ClinicDto>(clinic);
            return Result<ClinicDto>.Success(clinicDto);
        }
        catch (Exception ex)
        {
            return Result<ClinicDto>.Failure(ex.Message);
        }
    }
}
