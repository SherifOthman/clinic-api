using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public record VerifyResetOtpCommand(string Email, string Otp) : IRequest<Result>;