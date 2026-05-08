using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Commands;

public record CreateSpecializationCommand(string NameEn, string NameAr, string? DescriptionEn, string? DescriptionAr) : IRequest<Result<Guid>>;

public class CreateSpecializationHandler : IRequestHandler<CreateSpecializationCommand, Result<Guid>>
{
    private readonly IReferenceRepository _reference;
    private readonly IUnitOfWork          _uow;

    public CreateSpecializationHandler(IReferenceRepository reference, IUnitOfWork uow)
    {
        _reference = reference;
        _uow       = uow;
    }

    public async Task<Result<Guid>> Handle(CreateSpecializationCommand req, CancellationToken ct)
    {
        var entity = new Specialization
        {
            NameEn = req.NameEn.Trim(), NameAr = req.NameAr.Trim(),
            DescriptionEn = req.DescriptionEn?.Trim(), DescriptionAr = req.DescriptionAr?.Trim(),
            IsActive = true,
        };
        _reference.AddSpecialization(entity);
        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success(entity.Id);
    }
}

public record UpdateSpecializationCommand(Guid Id, string NameEn, string NameAr, string? DescriptionEn, string? DescriptionAr, bool IsActive) : IRequest<Result>;

public class UpdateSpecializationHandler : IRequestHandler<UpdateSpecializationCommand, Result>
{
    private readonly IReferenceRepository _reference;
    private readonly IUnitOfWork          _uow;

    public UpdateSpecializationHandler(IReferenceRepository reference, IUnitOfWork uow)
    {
        _reference = reference;
        _uow       = uow;
    }

    public async Task<Result> Handle(UpdateSpecializationCommand req, CancellationToken ct)
    {
        var entity = await _reference.GetSpecializationByIdAsync(req.Id, ct);
        if (entity is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Specialization not found");
        entity.NameEn = req.NameEn.Trim(); entity.NameAr = req.NameAr.Trim();
        entity.DescriptionEn = req.DescriptionEn?.Trim(); entity.DescriptionAr = req.DescriptionAr?.Trim();
        entity.IsActive = req.IsActive;
        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success();
    }
}

public record DeleteSpecializationCommand(Guid Id) : IRequest<Result>;

public class DeleteSpecializationHandler : IRequestHandler<DeleteSpecializationCommand, Result>
{
    private readonly IReferenceRepository      _reference;
    private readonly ISpecializationRepository _specializations;
    private readonly IUnitOfWork               _uow;

    public DeleteSpecializationHandler(IReferenceRepository reference, ISpecializationRepository specializations, IUnitOfWork uow)
    {
        _reference       = reference;
        _specializations = specializations;
        _uow             = uow;
    }

    public async Task<Result> Handle(DeleteSpecializationCommand req, CancellationToken ct)
    {
        var entity = await _reference.GetSpecializationByIdAsync(req.Id, ct);
        if (entity is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Specialization not found");

        var doctorCount = await _specializations.CountDoctorsAsync(req.Id, ct);
        if (doctorCount > 0)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED,
                $"Cannot delete '{entity.NameEn}' — it is assigned to {doctorCount} doctor{(doctorCount == 1 ? "" : "s")}. Deactivate it instead.");

        _specializations.Delete(entity);
        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success();
    }
}
