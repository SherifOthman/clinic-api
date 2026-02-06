using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicOwnerRepository : BaseRepository<ClinicOwner>, IClinicOwnerRepository
{
    public ClinicOwnerRepository(ApplicationDbContext context) : base(context)
    {
    }
}
