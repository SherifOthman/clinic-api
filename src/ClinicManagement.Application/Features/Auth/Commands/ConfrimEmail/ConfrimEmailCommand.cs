using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfrimEmail;

public class ConfrimEmailCommand : IRequest<Result>
{
    public int UserId { get; set; } 
    public string Token { get; set; } = string.Empty;
}
