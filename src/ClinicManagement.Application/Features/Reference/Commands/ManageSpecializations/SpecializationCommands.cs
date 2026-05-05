using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Commands;

// ── Create ────────────────────────────────────────────────────────────────────

public record CreateSpecializationCommand(
    string  NameEn,
    string  NameAr,
    string? DescriptionEn,
    string? DescriptionAr
) : IRequest<Result<Guid>>;

public class CreateSpecializationHandler : IRequestHandler<CreateSpecializationCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    public CreateSpecializationHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(CreateSpecializationCommand req, CancellationToken ct)
    {
        var entity = new Specialization
        {
            NameEn        = req.NameEn.Trim(),
            NameAr        = req.NameAr.Trim(),
            DescriptionEn = req.DescriptionEn?.Trim(),
            DescriptionAr = req.DescriptionAr?.Trim(),
            IsActive      = true,
        };

        _uow.Reference.AddSpecialization(entity);
        await _uow.SaveChangesAsync(ct);
        _uow.Reference.InvalidateCache();

        return Result.Success(entity.Id);
    }
}

// ── Update ────────────────────────────────────────────────────────────────────

public record UpdateSpecializationCommand(
    Guid    Id,
    string  NameEn,
    string  NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool    IsActive
) : IRequest<Result>;

public class UpdateSpecializationHandler : IRequestHandler<UpdateSpecializationCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public UpdateSpecializationHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateSpecializationCommand req, CancellationToken ct)
    {
        var entity = await _uow.Reference.GetSpecializationByIdAsync(req.Id, ct);
        if (entity is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Specialization not found");

        entity.NameEn        = req.NameEn.Trim();
        entity.NameAr        = req.NameAr.Trim();
        entity.DescriptionEn = req.DescriptionEn?.Trim();
        entity.DescriptionAr = req.DescriptionAr?.Trim();
        entity.IsActive      = req.IsActive;

        await _uow.SaveChangesAsync(ct);
        _uow.Reference.InvalidateCache();

        return Result.Success();
    }
}

// ── Delete ────────────────────────────────────────────────────────────────────

public record DeleteSpecializationCommand(Guid Id) : IRequest<Result>;

public class DeleteSpecializationHandler : IRequestHandler<DeleteSpecializationCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public DeleteSpecializationHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteSpecializationCommand req, CancellationToken ct)
    {
        var entity = await _uow.Reference.GetSpecializationByIdAsync(req.Id, ct);
        if (entity is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Specialization not found");

        // Check if any doctor is assigned this specialization
        var doctorCount = await _uow.Specializations.CountDoctorsAsync(req.Id, ct);
        if (doctorCount > 0)
            return Result.Failure(
                ErrorCodes.OPERATION_NOT_ALLOWED,
                $"Cannot delete '{entity.NameEn}' — it is assigned to {doctorCount} doctor{(doctorCount == 1 ? "" : "s")}. Deactivate it instead.");

        _uow.Specializations.Delete(entity);
        await _uow.SaveChangesAsync(ct);
        _uow.Reference.InvalidateCache();

        return Result.Success();
    }
}
