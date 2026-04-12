using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands;

public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateBranchHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var branch = new ClinicBranch
        {
            ClinicId     = clinicId,
            Name         = request.Name,
            AddressLine  = request.AddressLine,
            CityNameEn   = request.CityNameEn,
            CityNameAr   = request.CityNameAr,
            StateNameEn  = request.StateNameEn,
            StateNameAr  = request.StateNameAr,
            IsMainBranch = false,
            IsActive     = true,
            PhoneNumbers = request.PhoneNumbers
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => new ClinicBranchPhoneNumber { PhoneNumber = p.Trim() })
                .ToList(),
        };

        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Success(branch.Id);
    }
}
