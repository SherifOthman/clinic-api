using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetSuperAdminStatsHandler : IRequestHandler<GetSuperAdminStatsQuery, Result<SuperAdminStatsDto>>
{
    private readonly IUnitOfWork _uow;

    public GetSuperAdminStatsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<SuperAdminStatsDto>> Handle(
        GetSuperAdminStatsQuery request, CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters counts — these need dedicated repo methods (can't use base CountAsync)
        var totalClinics  = await _uow.Clinics.CountIgnoreFiltersAsync(cancellationToken);
        var totalPatients = await _uow.Patients.CountIgnoreFiltersAsync(cancellationToken);
        var totalStaff    = await _uow.Staff.CountActiveIgnoreFiltersAsync(cancellationToken);
        var clinicsOnTrial = await _uow.ClinicSubscriptions.CountByStatusIgnoreFiltersAsync(SubscriptionStatus.Trial, cancellationToken);
        var clinicsActive  = await _uow.ClinicSubscriptions.CountByStatusIgnoreFiltersAsync(SubscriptionStatus.Active, cancellationToken);

        return Result.Success(new SuperAdminStatsDto(
            TotalClinics:   totalClinics,
            TotalPatients:  totalPatients,
            TotalStaff:     totalStaff,
            ClinicsOnTrial: clinicsOnTrial,
            ClinicsActive:  clinicsActive));
    }
}
