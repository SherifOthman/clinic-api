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

    public async Task<List<Testimonial>> GetApprovedRandomAsync(int count, CancellationToken ct = default)
    {
        // No date seed — reviews are rare, just shuffle randomly each request.
        var all = await _set.Include(t => t.User)
                            .Include(t => t.Clinic)
                            .Where(t => t.IsApproved)
                            .ToListAsync(ct);

        return all.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }

    public async Task<(List<Testimonial> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _set.Include(t => t.User)
                        .Include(t => t.Clinic)
                        .OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

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
