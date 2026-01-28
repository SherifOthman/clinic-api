using ClinicManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;

public class UpdateProfileImageCommand : IRequest<Result<UpdateProfileImageResponse>>
{
    public IFormFile Image { get; set; } = null!;
}

public class UpdateProfileImageResponse
{
    public string ImageUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}