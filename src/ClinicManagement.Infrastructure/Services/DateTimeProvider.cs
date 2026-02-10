using ClinicManagement.Application.Common.Interfaces;

namespace ClinicManagement.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
