using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands;

/// <summary>Phone number with an optional label (e.g. "Reception", "Emergency").</summary>
public record BranchPhoneInput(string PhoneNumber, string? Label = null);

public record CreateBranchCommand(
    string Name,
    string AddressLine,
    int? StateGeonameId,
    int? CityGeonameId,
    List<BranchPhoneInput> PhoneNumbers
) : IRequest<Result<Guid>>;
