using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Auth.Commands;

public record LogoutCommand(string? RefreshToken) : IRequest<Result>;
