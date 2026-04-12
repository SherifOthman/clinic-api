using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands;

public record LogoutCommand(string? RefreshToken) : IRequest<Result>;
