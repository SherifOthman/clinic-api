using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedPatientsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetPatientsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedPatientsResponse>> Handle(
        GetPatientsQuery request,
        CancellationToken cancellationToken)
    {
        // No Include — PrimaryPhone is denormalized on Patient, phones only needed for detail view
        var query = _context.Patients
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm;
            var matchingPatientIds = _context.PatientPhones
                .Where(ph => ph.PhoneNumber.Contains(term))
                .Select(ph => ph.PatientId);

            query = query.Where(p =>
                p.FullName.Contains(term) ||
                p.PatientCode.Contains(term) ||
                matchingPatientIds.Contains(p.Id));
        }

        if (request.IsMale.HasValue)
        {
            query = query.Where(p => p.IsMale == request.IsMale.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "desc"
                ? query.OrderByDescending(p => p.FullName)
                : query.OrderBy(p => p.FullName),
            "createdat" => request.SortDirection == "desc"
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var patients = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<PatientDto>()
            .ToListAsync(cancellationToken);

        var response = new PaginatedPatientsResponse
        {
            Items = patients,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Result<PaginatedPatientsResponse>.Success(response);
    }
}
