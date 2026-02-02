using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Admin.Queries.GetClinic;

public class GetClinicQueryHandler : IRequestHandler<GetClinicQuery, Result<ClinicDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetClinicQueryHandler> _logger;

    public GetClinicQueryHandler(IUnitOfWork unitOfWork, ILogger<GetClinicQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ClinicDto>> Handle(GetClinicQuery request, CancellationToken cancellationToken)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(request.Id, cancellationToken);
        
        if (clinic == null)
        {
            _logger.LogWarning("Clinic with ID {Id} not found", request.Id);
            return Result<ClinicDto>.Fail(MessageCodes.Business.ENTITY_NOT_FOUND);
        }

        var dto = clinic.Adapt<ClinicDto>();
        return Result<ClinicDto>.Ok(dto);
    }
}
