using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.DeleteMedicine;

public record DeleteMedicineCommand(Guid Id) : IRequest<Result>;

public class DeleteMedicineCommandHandler : IRequestHandler<DeleteMedicineCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteMedicineCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = await _context.Medicines.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (medicine == null)
        {
            return Result.Fail("Medicine not found.");
        }

        _context.Medicines.Remove(medicine);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}