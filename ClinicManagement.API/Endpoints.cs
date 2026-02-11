using ClinicManagement.API.Common;
using System.Reflection;

namespace ClinicManagement.API;

/// <summary>
/// Auto-discovers and registers all endpoints implementing IEndpoint
/// Supports Vertical Slice Architecture pattern
/// </summary>
public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // Use reflection to find all IEndpoint implementations
        var endpointTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            var mapMethod = type.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            mapMethod?.Invoke(null, new object[] { api });
        }
    }
}
