using ClinicManagement.API.Common;
using System.Reflection;

namespace ClinicManagement.API;

/// <summary>
/// Central endpoint registration for Vertical Slice Architecture.
/// Each feature is organized by business capability.
/// Uses IEndpoint interface for explicit, discoverable endpoint mapping.
/// </summary>
public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // Map all endpoints that implement IEndpoint
        // This uses reflection to discover and register all endpoints
        var endpointTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            // Call the static Map method
            var mapMethod = type.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            mapMethod?.Invoke(null, new object[] { api });
        }
    }
}
