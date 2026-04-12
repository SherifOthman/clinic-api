using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands;

public record CreateBranchCommand(
    string Name,
    string AddressLine,
    string? CityNameEn,
    string? CityNameAr,
    string? StateNameEn,
    string? StateNameAr,
    List<string> PhoneNumbers
) : IRequest<Result<Guid>>;
