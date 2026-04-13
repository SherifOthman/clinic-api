using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands;

public record UpdateBranchCommand(
    Guid Id,
    string Name,
    string AddressLine,
    int? StateGeonameId,
    int? CityGeonameId,
    List<string> PhoneNumbers
) : IRequest<Result>;
