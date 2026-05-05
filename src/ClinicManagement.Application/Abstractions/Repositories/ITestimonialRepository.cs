using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface ITestimonialRepository
{
    Task<List<Testimonial>> GetApprovedAsync(CancellationToken ct = default);
    Task<List<Testimonial>> GetApprovedRandomAsync(int count, CancellationToken ct = default);
    Task<List<Testimonial>> GetAllAsync(CancellationToken ct = default);
    Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Testimonial?> GetByClinicIdAsync(Guid clinicId, CancellationToken ct = default);
    Task AddAsync(Testimonial testimonial, CancellationToken ct = default);
    void Update(Testimonial testimonial);
}
