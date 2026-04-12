using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands;

public class SetBranchActiveStatusHandler : IRequestHandler<SetBranchActiveStatusCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public SetBranchActiveStatusHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(SetBranchActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var branch = await _uow.Branches.GetByIdAsync(request.Id, cancellationToken);

        if (branch is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Branch not found");

        if (branch.IsMainBranch && !request.IsActive)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED, "Cannot deactivate the main branch");

        branch.IsActive = request.IsActive;
        branch.Touch();

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
