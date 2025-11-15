using AutoMapper;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, Result<List<MedicineDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMedicinesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<MedicineDto>>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        var medicines = await _unitOfWork.Medicines.SearchMedicinesAsync(request.SearchTerm, cancellationToken);
        var medicineDtos = _mapper.Map<List<MedicineDto>>(medicines);
        return Result<List<MedicineDto>>.Ok(medicineDtos);
    }
}
