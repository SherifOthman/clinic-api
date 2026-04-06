using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Domain.Enums;
using FsCheck;
using FsCheck.Xunit;

namespace ClinicManagement.Application.Tests.Patients;

/// <summary>
/// Property-based tests for CreatePatientCommandHandler.ParseBloodType.
/// Validates: Requirements 6.3
/// </summary>
public class CreatePatientCommandHandlerTests
{
    private static readonly Dictionary<string, BloodType> KnownMappings = new()
    {
        ["A+"]  = BloodType.APositive,
        ["A-"]  = BloodType.ANegative,
        ["B+"]  = BloodType.BPositive,
        ["B-"]  = BloodType.BNegative,
        ["AB+"] = BloodType.ABPositive,
        ["AB-"] = BloodType.ABNegative,
        ["O+"]  = BloodType.OPositive,
        ["O-"]  = BloodType.ONegative,
    };

    /// <summary>
    /// Property 1 (part a): Blood type parsing is total and correct — known strings return correct enum.
    /// Validates: Requirements 6.3
    /// </summary>
    [Property]
    public Property ParseBloodType_KnownStrings_ReturnCorrectEnum()
    {
        return Prop.ForAll(
            Gen.Elements(KnownMappings.Keys.ToArray()).ToArbitrary(),
            input => CreatePatientCommandHandler.ParseBloodType(input) == KnownMappings[input]
        );
    }

    /// <summary>
    /// Property 1 (part b): Blood type parsing is total and correct — unknown strings return null.
    /// Validates: Requirements 6.3
    /// </summary>
    [Property]
    public Property ParseBloodType_UnknownStrings_ReturnNull()
    {
        var knownStrings = new HashSet<string>(KnownMappings.Keys);
        return Prop.ForAll(
            Arb.Default.String().Filter(s => s != null && !knownStrings.Contains(s)),
            input => CreatePatientCommandHandler.ParseBloodType(input) == null
        );
    }

    /// <summary>
    /// Property 2: Generated patient codes are valid 8-digit strings.
    /// Validates: Requirements 6.4
    /// </summary>
    [Property(MaxTest = 100)]
    public Property GeneratePatientCode_AlwaysReturns8Digits()
    {
        return Prop.ForAll(
            Arb.Default.Unit(),
            _ =>
            {
                var code = CreatePatientCommandHandler.GeneratePatientCode();
                return code.Length == 8 && code.All(char.IsDigit);
            }
        );
    }
}
