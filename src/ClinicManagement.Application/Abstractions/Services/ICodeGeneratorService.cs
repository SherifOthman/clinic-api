namespace ClinicManagement.Application.Abstractions.Services;

public interface ICodeGeneratorService
{
    string GeneratePatientCode(int clinicId, int patientId);
}
