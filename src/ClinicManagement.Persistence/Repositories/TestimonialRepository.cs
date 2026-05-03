using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class TestimonialRepository : ITestimonialRepository
{
    private readonly DbSet<Testimonial> _set;

    public TestimonialRepository(ApplicationDbContext context)
        => _set = context.Set<Testimonial>();

    public Task<List<Testimonial>> GetApprovedAsync(CancellationToken ct = default)
        => _set.Include(t => t.User)
               .Include(t => t.Clinic)
               .Where(t => t.IsApproved)
               .OrderByDescending(t => t.CreatedAt)
               .ToListAsync(ct);

    public Task<List<Testimonial>> GetAllAsync(CancellationToken ct = default)
        => _set.Include(t => t.User)
               .Include(t => t.Clinic)
               .OrderByDescending(t => t.CreatedAt)
               .ToListAsync(ct);

    public Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.Include(t => t.User)
               .Include(t => t.Clinic)
               .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Testimonial?> GetByClinicIdAsync(Guid clinicId, CancellationToken ct = default)
        => _set.Include(t => t.User)
               .Include(t => t.Clinic)
               .FirstOrDefaultAsync(t => t.ClinicId == clinicId, ct);

    public async Task AddAsync(Testimonial testimonial, CancellationToken ct = default)
        => await _set.AddAsync(testimonial, ct);

    public void Update(Testimonial testimonial)
        => _set.Update(testimonial);
}
