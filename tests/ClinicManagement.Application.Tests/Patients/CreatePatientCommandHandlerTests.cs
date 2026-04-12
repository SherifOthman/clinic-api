using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Domain.Enums;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;
using FsCheck;
using FsCheck.Xunit;

namespace ClinicManagement.Application.Tests.Patients;

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

    [Property]
    public Property ParseBloodType_KnownStrings_ReturnCorrectEnum()
    {
        return Prop.ForAll(
            Gen.Elements(KnownMappings.Keys.ToArray()).ToArbitrary(),
            input => ParseBloodType(input) == KnownMappings[input]
        );
    }

    [Property]
    public Property ParseBloodType_UnknownStrings_ReturnNull()
    {
        var knownStrings = new HashSet<string>(KnownMappings.Keys);
        return Prop.ForAll(
            Arb.Default.String().Filter(s => s != null && !knownStrings.Contains(s)),
            input => ParseBloodType(input) == null
        );
    }

    [Property(MaxTest = 100)]
    public Property GeneratePatientCode_AlwaysReturns7Digits()
    {
        return Prop.ForAll(
            Arb.Default.Unit(),
            _ =>
            {
                var code = CreatePatientCommandHandler.GeneratePatientCode();
                return code.Length == 7 && code.All(char.IsDigit);
            }
        );
    }
}
