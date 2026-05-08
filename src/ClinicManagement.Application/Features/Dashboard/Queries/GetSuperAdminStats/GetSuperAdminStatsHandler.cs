using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetSuperAdminStatsHandler : IRequestHandler<GetSuperAdminStatsQuery, Result<SuperAdminStatsDto>>
{
    private readonly IClinicRepository             _clinics;
    private readonly IPatientRepository            _patients;
    private readonly IClinicMemberRepository       _members;
    private readonly IClinicSubscriptionRepository _subscriptions;

    public GetSuperAdminStatsHandler(
        IClinicRepository clinics,
        IPatientRepository patients,
        IClinicMemberRepository members,
        IClinicSubscriptionRepository subscriptions)
    {
        _clinics       = clinics;
        _patients      = patients;
        _members       = members;
        _subscriptions = subscriptions;
    }

    public async Task<Result<SuperAdminStatsDto>> Handle(
        GetSuperAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var totalClinics   = await _clinics.CountIgnoreFiltersAsync(cancellationToken);
        var totalPatients  = await _patients.CountIgnoreFiltersAsync(cancellationToken);
        var totalStaff     = await _members.CountActiveIgnoreFiltersAsync(cancellationToken);
        var clinicsOnTrial = await _subscriptions.CountByStatusIgnoreFiltersAsync(SubscriptionStatus.Trial, cancellationToken);
        var clinicsActive  = await _subscriptions.CountByStatusIgnoreFiltersAsync(SubscriptionStatus.Active, cancellationToken);

        return Result.Success(new SuperAdminStatsDto(
            TotalClinics:   totalClinics,
            TotalPatients:  totalPatients,
            TotalStaff:     totalStaff,
            ClinicsOnTrial: clinicsOnTrial,
            ClinicsActive:  clinicsActive));
    }
}
