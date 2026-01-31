using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetUserClinics;

public class GetUserClinicsQuery : IRequest<Result<IEnumerable<UserClinicDto>>>
{
}