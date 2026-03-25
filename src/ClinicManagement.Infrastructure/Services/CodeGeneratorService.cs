using ClinicManagement.Application.Abstractions.Services;

namespace ClinicManagement.Infrastructure.Services;

public class CodeGeneratorService : ICodeGeneratorService
{
    public string GeneratePatientCode(int clinicId, int patientId)
    {
        return $"P{clinicId:D4}-{patientId:D6}";
    }
}
