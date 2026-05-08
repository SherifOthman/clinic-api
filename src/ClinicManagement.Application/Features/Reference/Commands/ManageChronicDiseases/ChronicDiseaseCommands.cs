using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Commands;

public record CreateChronicDiseaseCommand(string NameEn, string NameAr, string? DescriptionEn, string? DescriptionAr) : IRequest<Result<Guid>>;

public class CreateChronicDiseaseHandler : IRequestHandler<CreateChronicDiseaseCommand, Result<Guid>>
{
    private readonly IReferenceRepository _reference;
    private readonly IUnitOfWork          _uow;

    public CreateChronicDiseaseHandler(IReferenceRepository reference, IUnitOfWork uow)
    {
        _reference = reference;
        _uow       = uow;
    }

    public async Task<Result<Guid>> Handle(CreateChronicDiseaseCommand req, CancellationToken ct)
    {
        var entity = new ChronicDisease
        {
            NameEn = req.NameEn.Trim(), NameAr = req.NameAr.Trim(),
            DescriptionEn = req.DescriptionEn?.Trim(), DescriptionAr = req.DescriptionAr?.Trim(),
            IsActive = true,
        };
        _reference.AddChronicDisease(entity);
        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success(entity.Id);
    }
}

public record UpdateChronicDiseaseCommand(Guid Id, string NameEn, string NameAr, string? DescriptionEn, string? DescriptionAr, bool IsActive) : IRequest<Result>;

public class UpdateChronicDiseaseHandler : IRequestHandler<UpdateChronicDiseaseCommand, Result>
{
    private readonly IReferenceRepository _reference;
    private readonly IUnitOfWork          _uow;

    public UpdateChronicDiseaseHandler(IReferenceRepository reference, IUnitOfWork uow)
    {
        _reference = reference;
        _uow       = uow;
    }

    public async Task<Result> Handle(UpdateChronicDiseaseCommand req, CancellationToken ct)
    {
        var entity = await _reference.GetChronicDiseaseByIdAsync(req.Id, ct);
        if (entity is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Chronic disease not found");
        entity.NameEn = req.NameEn.Trim(); entity.NameAr = req.NameAr.Trim();
        entity.DescriptionEn = req.DescriptionEn?.Trim(); entity.DescriptionAr = req.DescriptionAr?.Trim();
        entity.IsActive = req.IsActive;
        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success();
    }
}

public record DeleteChronicDiseaseCommand(Guid Id) : IRequest<Result>;

public class DeleteChronicDiseaseHandler : IRequestHandler<DeleteChronicDiseaseCommand, Result>
{
    private readonly IReferenceRepository    _reference;
    private readonly IChronicDiseaseRepository _chronicDiseases;
    private readonly IUnitOfWork             _uow;

    public DeleteChronicDiseaseHandler(IReferenceRepository reference, IChronicDiseaseRepository chronicDiseases, IUnitOfWork uow)
    {
        _reference       = reference;
        _chronicDiseases = chronicDiseases;
        _uow             = uow;
    }

    public async Task<Result> Handle(DeleteChronicDiseaseCommand req, CancellationToken ct)
    {
        var entity = await _reference.GetChronicDiseaseByIdAsync(req.Id, ct);
        if (entity is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Chronic disease not found");

        var patientCount = await _chronicDiseases.CountPatientsAsync(req.Id, ct);
        if (patientCount > 0)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED,
                $"Cannot delete '{entity.NameEn}' — it is assigned to {patientCount} patient{(patientCount == 1 ? "" : "s")}. Deactivate it instead.");

        _chronicDiseases.Delete(entity);
        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success();
    }
}
