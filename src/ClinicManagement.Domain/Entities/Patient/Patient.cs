using System;
using System.Collections.Generic;
using System.Linq;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Events;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Patient aggregate root - manages patient information, phone numbers, and chronic diseases
/// Enforces business rules and maintains consistency
/// </summary>
public class Patient : AggregateRoot
{
    // Private collections - can only be modified through methods
    private readonly List<PatientPhone> _phoneNumbers = [];
    private readonly List<PatientChronicDisease> _chronicDiseases = [];
    private readonly List<PatientAllergy> _allergies = [];

    // Private constructor for EF Core
    private Patient() { }

    public string PatientCode { get; private set; } = null!;
    public Guid ClinicId { get; private set; }
    public Clinic Clinic { get; set; } = null!;   
    public string FullName { get; private set; } = null!;
    public Gender Gender { get; private set; }
    public int? CityGeoNameId { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    
    // Critical medical information
    public string? BloodType { get; private set; }
    public string? KnownAllergies { get; private set; }  // Quick reference text field
    
    // Emergency contact
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public string? EmergencyContactRelation { get; private set; }
    
    // Read-only collections
    public IReadOnlyCollection<PatientPhone> PhoneNumbers => _phoneNumbers.AsReadOnly();
    public IReadOnlyCollection<PatientChronicDisease> ChronicDiseases => _chronicDiseases.AsReadOnly();
    public IReadOnlyCollection<PatientAllergy> Allergies => _allergies.AsReadOnly();
    
    // Navigation properties (for queries only - not part of aggregate)
    public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    
    // Calculated properties - pure business logic
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - 
        (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    
    public bool IsAdult => Age >= 18;
    public bool IsMinor => Age < 18;
    public bool IsSenior => Age >= 65;
    
    public string PrimaryPhoneNumber => _phoneNumbers.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber 
        ?? _phoneNumbers.FirstOrDefault()?.PhoneNumber 
        ?? string.Empty;
    
    public bool HasChronicDiseases => _chronicDiseases.Any();
    public int ChronicDiseaseCount => _chronicDiseases.Count;

    /// <summary>
    /// Factory method to create a new patient
    /// Ensures all invariants are met and raises domain event
    /// </summary>
    public static Patient Create(
        string patientCode,
        Guid clinicId,
        string fullName,
        Gender gender,
        DateTime dateOfBirth,
        int? cityGeoNameId = null)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(patientCode))
            throw new InvalidBusinessOperationException("Patient code is required");
        
        if (clinicId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic ID is required");
        
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidBusinessOperationException("Full name is required");
        
        if (dateOfBirth > DateTime.UtcNow)
            throw new InvalidBusinessOperationException("Date of birth cannot be in the future");
        
        if (dateOfBirth < DateTime.UtcNow.AddYears(-150))
            throw new InvalidBusinessOperationException("Date of birth is too far in the past");

        var patient = new Patient
        {
            PatientCode = patientCode,
            ClinicId = clinicId,
            FullName = fullName,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            CityGeoNameId = cityGeoNameId
        };

        // Raise domain event - dispatched after SaveChanges
        patient.AddDomainEvent(new PatientRegisteredEvent(
            patient.Id,
            patient.ClinicId,
            patient.PatientCode,
            patient.FullName,
            patient.PrimaryPhoneNumber,
            patient.DateOfBirth
        ));

        return patient;
    }

    /// <summary>
    /// Updates patient basic information
    /// </summary>
    public void UpdateInfo(string fullName, Gender gender, DateTime dateOfBirth, int? cityGeoNameId)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidBusinessOperationException("Full name is required");
        
        if (dateOfBirth > DateTime.UtcNow)
            throw new InvalidBusinessOperationException("Date of birth cannot be in the future");

        FullName = fullName;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        CityGeoNameId = cityGeoNameId;
    }

    /// <summary>
    /// Adds a phone number to the patient
    /// </summary>
    public void AddPhoneNumber(string phoneNumber, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidBusinessOperationException("Phone number is required");

        // Check for duplicate
        if (_phoneNumbers.Any(p => p.PhoneNumber == phoneNumber))
            throw new InvalidBusinessOperationException(
                $"Phone number {phoneNumber} already exists for this patient");

        // If setting as primary, unset other primary phones
        if (isPrimary)
        {
            foreach (var phone in _phoneNumbers)
            {
                phone.IsPrimary = false;
            }
        }
        // If this is the first phone, make it primary
        else if (!_phoneNumbers.Any())
        {
            isPrimary = true;
        }

        _phoneNumbers.Add(new PatientPhone
        {
            PatientId = Id,
            PhoneNumber = phoneNumber,
            IsPrimary = isPrimary
        });
    }

    /// <summary>
    /// Removes a phone number from the patient
    /// </summary>
    public void RemovePhoneNumber(string phoneNumber)
    {
        var phone = _phoneNumbers.FirstOrDefault(p => p.PhoneNumber == phoneNumber);
        if (phone == null)
            throw new InvalidBusinessOperationException(
                $"Phone number {phoneNumber} not found");

        // Don't allow removing the last phone number
        if (_phoneNumbers.Count == 1)
            throw new InvalidBusinessOperationException(
                "Cannot remove the last phone number. Patient must have at least one phone number.");

        var wasPrimary = phone.IsPrimary;
        _phoneNumbers.Remove(phone);

        // If we removed the primary phone, make the first remaining phone primary
        if (wasPrimary && _phoneNumbers.Any())
        {
            _phoneNumbers.First().IsPrimary = true;
        }
    }

    /// <summary>
    /// Sets a phone number as primary
    /// </summary>
    public void SetPrimaryPhoneNumber(string phoneNumber)
    {
        var phone = _phoneNumbers.FirstOrDefault(p => p.PhoneNumber == phoneNumber);
        if (phone == null)
            throw new InvalidBusinessOperationException(
                $"Phone number {phoneNumber} not found");

        // Unset all other primary phones
        foreach (var p in _phoneNumbers)
        {
            p.IsPrimary = false;
        }

        phone.IsPrimary = true;
    }

    /// <summary>
    /// Associates a chronic disease with the patient
    /// </summary>
    public void AddChronicDisease(Guid chronicDiseaseId)
    {
        if (chronicDiseaseId == Guid.Empty)
            throw new InvalidBusinessOperationException("Chronic disease ID is required");

        // Check for duplicate
        if (_chronicDiseases.Any(cd => cd.ChronicDiseaseId == chronicDiseaseId))
            throw new InvalidBusinessOperationException(
                "This chronic disease is already associated with the patient");

        _chronicDiseases.Add(new PatientChronicDisease
        {
            PatientId = Id,
            ChronicDiseaseId = chronicDiseaseId
        });
    }

    /// <summary>
    /// Removes a chronic disease association from the patient
    /// </summary>
    public void RemoveChronicDisease(Guid chronicDiseaseId)
    {
        var disease = _chronicDiseases.FirstOrDefault(cd => cd.ChronicDiseaseId == chronicDiseaseId);
        if (disease == null)
            throw new InvalidBusinessOperationException(
                "Chronic disease association not found");

        _chronicDiseases.Remove(disease);
    }

    /// <summary>
    /// Adds an allergy to the patient (CRITICAL for prescription safety)
    /// </summary>
    public void AddAllergy(
        string allergyName,
        AllergySeverity severity,
        string? reaction = null,
        DateTime? diagnosedAt = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(allergyName))
            throw new InvalidBusinessOperationException("Allergy name is required");

        // Check for duplicate
        if (_allergies.Any(a => a.AllergyName.Equals(allergyName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidBusinessOperationException(
                $"Allergy '{allergyName}' is already recorded for this patient");

        _allergies.Add(new PatientAllergy
        {
            PatientId = Id,
            AllergyName = allergyName,
            Severity = severity,
            Reaction = reaction,
            DiagnosedAt = diagnosedAt,
            Notes = notes
        });

        // Update quick reference field
        UpdateKnownAllergiesText();
    }

    /// <summary>
    /// Removes an allergy from the patient
    /// </summary>
    public void RemoveAllergy(string allergyName)
    {
        var allergy = _allergies.FirstOrDefault(a => a.AllergyName.Equals(allergyName, StringComparison.OrdinalIgnoreCase));
        if (allergy == null)
            throw new InvalidBusinessOperationException(
                $"Allergy '{allergyName}' not found");

        _allergies.Remove(allergy);

        // Update quick reference field
        UpdateKnownAllergiesText();
    }

    /// <summary>
    /// Updates emergency contact information
    /// </summary>
    public void UpdateEmergencyContact(string? name, string? phone, string? relation)
    {
        EmergencyContactName = name;
        EmergencyContactPhone = phone;
        EmergencyContactRelation = relation;
    }

    /// <summary>
    /// Updates blood type
    /// </summary>
    public void UpdateBloodType(string? bloodType)
    {
        BloodType = bloodType;
    }

    /// <summary>
    /// Updates the quick reference allergies text field
    /// </summary>
    private void UpdateKnownAllergiesText()
    {
        if (!_allergies.Any())
        {
            KnownAllergies = null;
            return;
        }

        KnownAllergies = string.Join(", ", _allergies.Select(a => a.AllergyName));
    }

    /// <summary>
    /// Checks if patient has a specific allergy (for prescription safety checks)
    /// </summary>
    public bool HasAllergy(string allergyName)
    {
        return _allergies.Any(a => a.AllergyName.Equals(allergyName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all severe or life-threatening allergies
    /// </summary>
    public IEnumerable<PatientAllergy> GetCriticalAllergies()
    {
        return _allergies.Where(a => a.Severity == AllergySeverity.Severe || a.Severity == AllergySeverity.LifeThreatening);
    }
}

