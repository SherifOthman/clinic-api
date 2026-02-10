using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Specializations.Queries.GetSpecializationById;

public record GetSpecializationByIdQuery(Guid Id) : IRequest<Result<SpecializationDto>>;

public class GetSpecializationByIdQueryHandler : IRequestHandler<GetSpecializationByIdQuery, Result<SpecializationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSpecializationByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SpecializationDto>> Handle(GetSpecializationByIdQuery request, CancellationToken cancellationToken)
    {
        var specialization = await _unitOfWork.Specializations.GetByIdAsync(request.Id, cancellationToken);
        
        if (specialization == null)
        {
            return Result<SpecializationDto>.FailSystem("NOT_FOUND", "Specialization not found");
        }

        var specializationDto = specialization.Adapt<SpecializationDto>();
        return Result<SpecializationDto>.Ok(specializationDto);
    }
}