using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Admin.Queries.GetClinics;

public class GetClinicsQueryHandler : IRequestHandler<GetClinicsQuery, Result<PagedResult<ClinicDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetClinicsQueryHandler> _logger;

    public GetClinicsQueryHandler(
        IUnitOfWork unitOfWork, 
        ILogger<GetClinicsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ClinicDto>>> Handle(GetClinicsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var clinicSearchRequest = new ClinicSearchRequest(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.SortBy,
                request.SortDescending)
            {
                SubscriptionPlanId = request.SubscriptionPlanId,
                IsActive = request.IsActive,
                CreatedFrom = request.CreatedFrom,
                CreatedTo = request.CreatedTo,
                MinUsers = request.MinUsers,
                MaxUsers = request.MaxUsers
            };

            var clinics = await _unitOfWork.Clinics.GetPagedAsync(clinicSearchRequest, cancellationToken);
            
            var clinicDtos = clinics.Items.Adapt<List<ClinicDto>>();
            
            var result = new PagedResult<ClinicDto>(
                clinicDtos,
                clinics.TotalCount,
                clinics.PageNumber,
                clinics.PageSize
            );

            _logger.LogInformation("Retrieved {Count} clinics for admin with search term: {SearchTerm}", 
                clinicDtos.Count, request.SearchTerm);
            return Result<PagedResult<ClinicDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clinics for admin");
            return Result<PagedResult<ClinicDto>>.Fail("Failed to retrieve clinics");
        }
    }
}
