using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery(GetUsersRequest Request) : IRequest<Result<PagedResult<UserDto>>>;
