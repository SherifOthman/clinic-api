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
        var clinicId = _currentUser.GetRequiredClinicId();

        var query = _context.Patients
            .Include(p => p.Phones)
            .Where(p => p.ClinicId == clinicId && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p =>
                p.FullName.Contains(request.SearchTerm) ||
                p.PatientCode.Contains(request.SearchTerm) ||
                p.Phones.Any(ph => ph.PhoneNumber.Contains(request.SearchTerm)));
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
