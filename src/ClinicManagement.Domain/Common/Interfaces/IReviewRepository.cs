using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetReviewsWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Review?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
