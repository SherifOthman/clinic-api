using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicManagement.Application.Features.Auth.Commands.UploadProfileImage;

public record UploadProfileImageCommand(
    IFormFile File
) : IRequest<UploadProfileImageResult>;
