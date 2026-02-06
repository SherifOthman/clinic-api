using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ReceptionistRepository : BaseRepository<Receptionist>, IReceptionistRepository
{
    public ReceptionistRepository(ApplicationDbContext context) : base(context)
    {
    }
}
