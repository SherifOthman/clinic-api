using ClinicManagement.Application.Common.Extensions;
using ClinicManagement.Application.Common.Interfaces;
using FluentValidation;

namespace ClinicManagement.Application.Features.Admin.Queries.GetClinics;

public class GetClinicsQueryValidator : AbstractValidator<GetClinicsQuery>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetClinicsQueryValidator(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;

        RuleFor(x => x.PageNumber)
            .ValidPageNumber();

        RuleFor(x => x.PageSize)
            .ValidPageSize();

        RuleFor(x => x.SearchTerm)
            .ValidOptionalString(100);

        RuleFor(x => x.MinUsers)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinUsers.HasValue)
            .WithMessage("Minimum users must be 0 or greater");

        RuleFor(x => x.MaxUsers)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxUsers.HasValue)
            .WithMessage("Maximum users must be 0 or greater");

        RuleFor(x => x)
            .Must(x => !x.MinUsers.HasValue || !x.MaxUsers.HasValue || x.MinUsers <= x.MaxUsers)
            .WithMessage("Minimum users cannot be greater than maximum users");

        RuleFor(x => x.CreatedFrom)
            .ValidDateRange(DateTime.MinValue, _dateTimeProvider.UtcNow.AddDays(1));

        RuleFor(x => x.CreatedTo)
            .ValidDateRange(DateTime.MinValue, _dateTimeProvider.UtcNow.AddDays(1));

        RuleFor(x => x)
            .Must(x => !x.CreatedFrom.HasValue || !x.CreatedTo.HasValue || x.CreatedFrom <= x.CreatedTo)
            .WithMessage("Created from date cannot be after created to date");
    }
}
