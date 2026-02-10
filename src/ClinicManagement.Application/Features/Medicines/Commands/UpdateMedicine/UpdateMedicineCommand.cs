using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.UpdateMedicine;

public record UpdateMedicineCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal BoxPrice { get; init; }
    public int StripsPerBox { get; init; }
    public int TotalStripsInStock { get; init; }
    public int MinimumStockLevel { get; init; }
}

public class UpdateMedicineCommandHandler : IRequestHandler<UpdateMedicineCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateMedicineCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = await _context.Medicines.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (medicine == null)
        {
            return Result.Fail("Medicine not found.");
        }

        medicine.Name = request.Name;
        medicine.BoxPrice = request.BoxPrice;
        medicine.StripsPerBox = request.StripsPerBox;
        medicine.TotalStripsInStock = request.TotalStripsInStock;
        medicine.MinimumStockLevel = request.MinimumStockLevel;

        _context.Medicines.Update(medicine);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}