using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands;

public class UpdateBranchHandler : IRequestHandler<UpdateBranchCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public UpdateBranchHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _uow.Branches.GetByIdWithPhonesAsync(request.Id, cancellationToken);

        if (branch is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Branch not found");

        branch.Name           = request.Name;
        branch.AddressLine    = request.AddressLine;
        branch.StateGeonameId = request.StateGeonameId;
        branch.CityGeonameId  = request.CityGeonameId;

        // Replace phone numbers
        branch.PhoneNumbers.Clear();
        foreach (var p in request.PhoneNumbers.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)))
            branch.PhoneNumbers.Add(new ClinicBranchPhoneNumber { PhoneNumber = p.PhoneNumber.Trim(), Label = p.Label?.Trim() });

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
