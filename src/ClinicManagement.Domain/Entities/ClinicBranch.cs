
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public int Phone {get;set; }
    public int CityId { get; set; }
    public City City { get; set; } = null!;
}
