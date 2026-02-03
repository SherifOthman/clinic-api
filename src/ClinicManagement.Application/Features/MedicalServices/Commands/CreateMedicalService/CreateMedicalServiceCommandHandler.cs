using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalServices.Commands.CreateMedicalService;

public class CreateMedicalServiceCommandHandler : IRequestHandler<CreateMedicalServiceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateMedicalServiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMedicalServiceCommand request, CancellationToken cancellationToken)
    {
        var service = new MedicalService
        {
            Id = Guid.NewGuid(),
            ClinicBranchId = request.ClinicBranchId,
            Name = request.Name,
            DefaultPrice = request.DefaultPrice,
            IsOperation = request.IsOperation
        };

        await _unitOfWork.MedicalServices.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(service.Id);
    }
}