namespace ClinicManagement.Application.DTOs;

public class AppointmentTypeDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
}

public class CreateAppointmentTypeDto
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
}

public class UpdateAppointmentTypeDto
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
}
