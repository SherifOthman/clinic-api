using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicRepository : BaseRepository<Clinic>, IClinicRepository
{
    public ClinicRepository(ApplicationDbContext context) : base(context)
    {
    }
}
