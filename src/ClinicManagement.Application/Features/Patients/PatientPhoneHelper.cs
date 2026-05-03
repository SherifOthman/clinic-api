using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.Patients;

/// <summary>
/// Shared helpers for patient phone number operations.
/// Both CreatePatient and UpdatePatient normalize and attach phones the same way.
/// </summary>
internal static class PatientPhoneHelper
{
    /// <summary>
    /// Clears all existing phones on the patient then adds the new list.
    /// Pass <paramref name="existingPhones"/> as null to skip the clear step (create path).
    /// </summary>
    public static void ReplacePhones(
        IPatientRepository patients,
        IPhoneNormalizer normalizer,
        Guid patientId,
        IEnumerable<string> newPhones,
        string? countryCode,
        IEnumerable<PatientPhone>? existingPhones = null)
    {
        if (existingPhones is not null)
            foreach (var phone in existingPhones.ToList())
                patients.RemovePhone(phone);

        foreach (var phone in newPhones)
            patients.AddPhone(new PatientPhone
            {
                PatientId      = patientId,
                PhoneNumber    = phone,
                NationalNumber = normalizer.GetNationalNumber(phone, countryCode) ?? phone,
            });
    }
}
