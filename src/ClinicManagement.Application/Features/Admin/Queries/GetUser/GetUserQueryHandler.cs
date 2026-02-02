using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Admin.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserQueryHandler> _logger;

    public GetUserQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
            }

            var userDto = user.Adapt<UserDto>();
            
            _logger.LogInformation("Retrieved user {UserId} for admin", request.Id);
            return Result<UserDto>.Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId} for admin", request.Id);
            return Result<UserDto>.Fail(MessageCodes.Business.INVALID_OPERATION);
        }
    }
}
