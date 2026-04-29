using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IDoctorSessionRepository
{
    Task<DoctorSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DoctorSession?> GetByDoctorBranchDateAsync(Guid doctorInfoId, Guid branchId, DateOnly date, CancellationToken ct = default);
    Task<List<DoctorSession>> GetByBranchAndDateAsync(Guid branchId, DateOnly date, CancellationToken ct = default);
    Task AddAsync(DoctorSession session, CancellationToken ct = default);
    void Update(DoctorSession session);
}
