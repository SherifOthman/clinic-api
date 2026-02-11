namespace ClinicManagement.API.Common;

/// <summary>
/// Represents a minimal API endpoint.
/// Each endpoint is a self-contained vertical slice with its own request, response, validation, and handler.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the route builder.
    /// </summary>
    static abstract void Map(IEndpointRouteBuilder app);
}
