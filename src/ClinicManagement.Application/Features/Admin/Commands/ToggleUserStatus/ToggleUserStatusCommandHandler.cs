using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Admin.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ToggleUserStatusCommandHandler> _logger;

    public ToggleUserStatusCommandHandler(IUnitOfWork unitOfWork, ILogger<ToggleUserStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for status toggle", request.Id);
                return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
            }

            // Toggle email confirmed status (this could be extended to other status fields)
            user.EmailConfirmed = !user.EmailConfirmed;

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = user.Adapt<UserDto>();
            
            _logger.LogInformation("Toggled status for user {UserId} by admin. EmailConfirmed: {EmailConfirmed}", 
                request.Id, user.EmailConfirmed);
            return Result<UserDto>.Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for user {UserId} by admin", request.Id);
            return Result<UserDto>.Fail(MessageCodes.Business.INVALID_OPERATION);
        }
    }
}
