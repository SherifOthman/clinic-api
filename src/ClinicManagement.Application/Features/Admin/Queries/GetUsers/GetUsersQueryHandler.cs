using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUsersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;
            var userSearchRequest = new UserSearchRequest(
                req.PageNumber,
                req.PageSize,
                req.SearchTerm,
                req.SortBy,
                req.SortDescending)
            {
                EmailConfirmed = req.EmailConfirmed
            };

            var users = await _unitOfWork.Users.GetPagedAsync(userSearchRequest, cancellationToken);

            var userDtos = users.Items.Adapt<List<UserDto>>();
            
            var result = new PagedResult<UserDto>(
                userDtos,
                users.TotalCount,
                users.PageNumber,
                users.PageSize
            );

            _logger.LogInformation("Retrieved {Count} users for admin with filters: SearchTerm={SearchTerm}, Role={Role}, ClinicId={ClinicId}", 
                userDtos.Count, req.SearchTerm, req.Role, req.ClinicId);
            return Result<PagedResult<UserDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for admin");
            return Result<PagedResult<UserDto>>.Fail(MessageCodes.Admin.USERS_RETRIEVAL_FAILED);
        }
    }
}
