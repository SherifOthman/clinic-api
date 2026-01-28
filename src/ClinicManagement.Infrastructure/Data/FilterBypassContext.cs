namespace ClinicManagement.Infrastructure.Data;

public class FilterBypassContext
{
    private static readonly AsyncLocal<bool> _bypassFilters = new();
    
    public static bool ShouldBypassFilters => _bypassFilters.Value;
    
    public static IDisposable BypassFilters()
    {
        _bypassFilters.Value = true;
        return new FilterBypassScope();
    }
    
    private class FilterBypassScope : IDisposable
    {
        public void Dispose()
        {
            _bypassFilters.Value = false;
        }
    }
}