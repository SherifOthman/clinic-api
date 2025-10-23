using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Clinics.Commands.CreateClinic;

public class CreateClinicCommandHandler : IRequestHandler<CreateClinicCommand, Result<ClinicDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateClinicCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(CreateClinicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = new Clinic
            {
                Name = request.Name,
                Phone = request.Phone,
                OwnerId = request.OwnerId,
                SubscriptionPlanId = request.SubscriptionPlanId,
                StartDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.UnitOfWork.Clinics.Add(clinic);
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
