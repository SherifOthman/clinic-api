using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicManagement.Application.Features.Auth.Commands;

public record UploadProfileImageCommand(IFormFile File) : IRequest<Result>;
