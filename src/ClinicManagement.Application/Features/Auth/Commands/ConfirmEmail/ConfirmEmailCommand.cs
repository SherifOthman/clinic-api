using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailCommand : IRequest<Result>
{
    public int UserId { get; set; } 
    public string Token { get; set; } = string.Empty;
}
