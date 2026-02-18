using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : TenantEntity
{
    
    public int CountryGeoNameId { get; set; }
    public int StateGeoNameId { get; set; }
    public int CityGeoNameId { get; set; }
}
