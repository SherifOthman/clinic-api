using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    // No additional methods needed - everything goes through GetPagedAsync
}
