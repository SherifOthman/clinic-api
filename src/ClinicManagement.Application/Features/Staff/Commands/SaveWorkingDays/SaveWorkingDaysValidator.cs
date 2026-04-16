using FluentValidation;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SaveWorkingDaysValidator : AbstractValidator<SaveWorkingDaysCommand>
{
    public SaveWorkingDaysValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required");

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleForEach(x => x.Days).ChildRules(day =>
        {
            day.RuleFor(d => d.Day)
                .InclusiveBetween(0, 6).WithMessage("Day must be between 0 (Sunday) and 6 (Saturday)");

            day.RuleFor(d => d.StartTime)
                .NotEmpty().WithMessage("Start time is required")
                .Matches(@"^\d{2}:\d{2}$").WithMessage("Start time must be in HH:mm format");

            day.RuleFor(d => d.EndTime)
                .NotEmpty().WithMessage("End time is required")
                .Matches(@"^\d{2}:\d{2}$").WithMessage("End time must be in HH:mm format");

            day.RuleFor(d => d)
                .Must(d => !d.IsAvailable || TimeOnly.Parse(d.StartTime) < TimeOnly.Parse(d.EndTime))
                .When(d => d.IsAvailable && d.StartTime.Length == 5 && d.EndTime.Length == 5)
                .WithMessage("Start time must be before end time");
        });
    }
}
