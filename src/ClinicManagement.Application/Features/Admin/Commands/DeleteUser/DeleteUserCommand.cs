using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Commands.DeleteUser;

public class DeleteUserCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
