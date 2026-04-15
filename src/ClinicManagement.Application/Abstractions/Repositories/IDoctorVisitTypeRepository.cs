using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IDoctorVisitTypeRepository
{
    Task<List<DoctorVisitType>> GetByDoctorAndBranchAsync(Guid doctorId, Guid branchId, CancellationToken ct = default);
    Task<DoctorVisitType?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasAppointmentsAsync(Guid doctorVisitTypeId, CancellationToken ct = default);
    void Add(DoctorVisitType visitType);
    void Remove(DoctorVisitType visitType);
}
