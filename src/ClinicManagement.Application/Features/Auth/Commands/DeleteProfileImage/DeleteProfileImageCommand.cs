using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands;

public record DeleteProfileImageCommand() : IRequest<Result>;
