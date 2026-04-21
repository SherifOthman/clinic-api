using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetMemberPermissionsQuery(Guid MemberId) : IRequest<List<Permission>?>;
