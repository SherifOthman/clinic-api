using AutoMapper;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;

public class CreateMedicineCommandHandler : IRequestHandler<CreateMedicineCommand, Result<MedicineDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateMedicineCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<MedicineDto>> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = new Medicine
        {
            Name = request.Name,
            GenericName = request.GenericName,
            Dosage = request.Dosage,
            Form = request.Form,
            Description = request.Description
        };
        
        _unitOfWork.Medicines.Add(medicine);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var medicineDto = _mapper.Map<MedicineDto>(medicine);
        return Result<MedicineDto>.Ok(medicineDto);
    }
}
