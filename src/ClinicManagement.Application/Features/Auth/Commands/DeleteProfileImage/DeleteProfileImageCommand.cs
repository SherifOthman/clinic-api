using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.DeleteProfileImage;

public record DeleteProfileImageCommand() : IRequest<DeleteProfileImageResult>;
