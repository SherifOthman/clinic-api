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
        // Seed the shuffle with today's date so the selection is stable within a day
        // but changes every day — same result for all visitors on the same day.
        var daySeed = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
        var rng     = new Random(daySeed);

        var all = await _set.Include(t => t.User)
                            .Include(t => t.Clinic)
                            .Where(t => t.IsApproved)
                            .ToListAsync(ct);

        return all.OrderBy(_ => rng.Next()).Take(count).ToList();
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
