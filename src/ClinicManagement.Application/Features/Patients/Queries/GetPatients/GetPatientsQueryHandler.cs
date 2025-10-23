using AutoMapper;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedList<PatientDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<PatientDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _unitOfWork.Patients.GetAll();

            // Apply filters
            if (request.ClinicId.HasValue)
            {
                query = query.Where(p => p.ClinicId == request.ClinicId.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.FirstName.ToLower().Contains(searchLower) ||
                    p.ThirdName.ToLower().Contains(searchLower) ||
                    (p.SecondName != null && p.SecondName.ToLower().Contains(searchLower)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(request.SearchTerm))
                );
            }

            if (request.Gender.HasValue)
            {
                query = query.Where(p => p.Gender == request.Gender.Value);
            }

            if (!string.IsNullOrEmpty(request.City))
            {
                query = query.Where(p => p.City == request.City);
            }

            // Age filters
            if (request.MinAge.HasValue)
            {
                var maxBirthDate = DateTime.Today.AddYears(-request.MinAge.Value);
                query = query.Where(p => p.DateOfBirth.HasValue && p.DateOfBirth <= maxBirthDate);
            }

            if (request.MaxAge.HasValue)
            {
                var minBirthDate = DateTime.Today.AddYears(-request.MaxAge.Value - 1);
                query = query.Where(p => p.DateOfBirth.HasValue && p.DateOfBirth >= minBirthDate);
            }

            // Order by name
            query = query.OrderBy(p => p.FirstName).ThenBy(p => p.ThirdName);

            // Create paginated result
            var paginatedPatients = await PaginatedList<Patient>.CreateAsync(
                query, request.PageNumber, request.PageSize, cancellationToken);

            var patientDtos = _mapper.Map<List<PatientDto>>(paginatedPatients.Items);
            
            var result = new PaginatedList<PatientDto>(
                patientDtos,
                paginatedPatients.TotalCount,
                paginatedPatients.PageNumber,
                paginatedPatients.PageSize
            );

            return Result<PaginatedList<PatientDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<PatientDto>>.Failure(ex.Message);
        }
    }
}
