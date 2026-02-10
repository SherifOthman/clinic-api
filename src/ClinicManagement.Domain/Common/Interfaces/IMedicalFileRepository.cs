using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicalFileRepository : IRepository<MedicalFile>
{
    Task<int> GetCountForClinicByYearAsync(Guid clinicId, int year, CancellationToken cancellationToken = default);
}
