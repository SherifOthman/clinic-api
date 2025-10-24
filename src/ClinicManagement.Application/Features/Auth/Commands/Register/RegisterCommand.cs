using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result>
{
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string ThirdName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
