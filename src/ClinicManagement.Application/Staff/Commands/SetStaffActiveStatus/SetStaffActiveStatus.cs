using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Commands;

public record SetStaffActiveStatusCommand(Guid StaffId, bool IsActive) : IRequest<Result>;

public class SetStaffActiveStatusHandler : IRequestHandler<SetStaffActiveStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetStaffActiveStatusHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SetStaffActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);

        if (staff == null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Staff member not found");

        staff.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
