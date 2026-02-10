using System;
using System.Linq;
using System.Linq.Expressions;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Specifications;

/// <summary>
/// Specification for patients with chronic diseases
/// </summary>
public class PatientWithChronicDiseaseSpecification : Specification<Patient>
{
    public override Expression<Func<Patient, bool>> ToExpression()
    {
        return patient => patient.ChronicDiseases.Any();
    }
}

/// <summary>
/// Specification for senior patients (age >= 65)
/// </summary>
public class SeniorPatientSpecification : Specification<Patient>
{
    public override Expression<Func<Patient, bool>> ToExpression()
    {
        var cutoffDate = DateTime.UtcNow.AddYears(-65);
        return patient => patient.DateOfBirth <= cutoffDate;
    }
}

/// <summary>
/// Specification for minor patients (age < 18)
/// </summary>
public class MinorPatientSpecification : Specification<Patient>
{
    public override Expression<Func<Patient, bool>> ToExpression()
    {
        var cutoffDate = DateTime.UtcNow.AddYears(-18);
        return patient => patient.DateOfBirth > cutoffDate;
    }
}

/// <summary>
/// Specification for patients needing follow-up (no visit in X months)
/// </summary>
public class PatientNeedingFollowUpSpecification : Specification<Patient>
{
    private readonly DateTime _cutoffDate;

    public PatientNeedingFollowUpSpecification(int monthsSinceLastVisit)
    {
        _cutoffDate = DateTime.UtcNow.AddMonths(-monthsSinceLastVisit);
    }

    public override Expression<Func<Patient, bool>> ToExpression()
    {
        var cutoffDate = _cutoffDate;
        return patient => patient.UpdatedAt < cutoffDate;
    }
}

/// <summary>
/// Specification for patients with specific chronic disease
/// </summary>
public class PatientWithSpecificDiseaseSpecification : Specification<Patient>
{
    private readonly Guid _chronicDiseaseId;

    public PatientWithSpecificDiseaseSpecification(Guid chronicDiseaseId)
    {
        _chronicDiseaseId = chronicDiseaseId;
    }

    public override Expression<Func<Patient, bool>> ToExpression()
    {
        var diseaseId = _chronicDiseaseId;
        return patient => patient.ChronicDiseases.Any(cd => cd.ChronicDiseaseId == diseaseId);
    }
}

/// <summary>
/// Specification for patients by gender
/// </summary>
public class PatientByGenderSpecification : Specification<Patient>
{
    private readonly Gender _gender;

    public PatientByGenderSpecification(Gender gender)
    {
        _gender = gender;
    }

    public override Expression<Func<Patient, bool>> ToExpression()
    {
        var gender = _gender;
        return patient => patient.Gender == gender;
    }
}

/// <summary>
/// Specification for patients in specific age range
/// </summary>
public class PatientInAgeRangeSpecification : Specification<Patient>
{
    private readonly DateTime _minDateOfBirth;
    private readonly DateTime _maxDateOfBirth;

    public PatientInAgeRangeSpecification(int minAge, int maxAge)
    {
        _maxDateOfBirth = DateTime.UtcNow.AddYears(-minAge);
        _minDateOfBirth = DateTime.UtcNow.AddYears(-maxAge);
    }

    public override Expression<Func<Patient, bool>> ToExpression()
    {
        var minDob = _minDateOfBirth;
        var maxDob = _maxDateOfBirth;
        return patient => patient.DateOfBirth >= minDob && patient.DateOfBirth <= maxDob;
    }
}

/// <summary>
/// Specification for patients with allergies
/// </summary>
public class PatientWithAllergiesSpecification : Specification<Patient>
{
    public override Expression<Func<Patient, bool>> ToExpression()
    {
        return patient => patient.Allergies.Any();
    }
}

/// <summary>
/// Specification for patients with specific allergy
/// </summary>
public class PatientWithSpecificAllergySpecification : Specification<Patient>
{
    private readonly string _allergyName;

    public PatientWithSpecificAllergySpecification(string allergyName)
    {
        _allergyName = allergyName;
    }

    public override Expression<Func<Patient, bool>> ToExpression()
    {
        var allergyName = _allergyName;
        return patient => patient.Allergies.Any(a => 
            a.AllergyName.ToLower().Contains(allergyName.ToLower()));
    }
}
