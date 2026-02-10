using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalServices.Queries.GetMedicalServices;

public record GetMedicalServicesQuery(Guid ClinicBranchId) : IRequest<Result<IEnumerable<MedicalServiceDto>>>;

public class GetMedicalServicesQueryHandler : IRequestHandler<GetMedicalServicesQuery, Result<IEnumerable<MedicalServiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicalServicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<MedicalServiceDto>>> Handle(GetMedicalServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _unitOfWork.MedicalServices.GetByClinicBranchIdAsync(request.ClinicBranchId, cancellationToken);
        var servicesDto = services.Adapt<IEnumerable<MedicalServiceDto>>();
        return Result<IEnumerable<MedicalServiceDto>>.Ok(servicesDto);
    }
}