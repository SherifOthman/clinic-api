using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicBranchPhoneNumberRepository : BaseRepository<ClinicBranchPhoneNumber>, IClinicBranchPhoneNumberRepository
{
    public ClinicBranchPhoneNumberRepository(ApplicationDbContext context) : base(context)
    {
    }
}
