using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ClinicManagement.API.OpenApi;

/// <summary>
/// Transforms the OpenAPI document to include Bearer JWT authentication scheme
/// This ensures Scalar UI displays the authentication input field
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider
) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document, 
        OpenApiDocumentTransformerContext context, 
        CancellationToken cancellationToken)
    {
        // Retrieve all registered authentication schemes
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        
        // Check if the application has the "Bearer" scheme defined
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            // Define the Bearer security scheme object
            var bearerScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
            };

            // Add the security scheme to the document components
            document.Components ??= new OpenApiComponents();
            document.AddComponent("Bearer", bearerScheme);

            // Create security requirement that references the Bearer scheme
            var securityRequirements = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            };

            // Apply this security requirement globally to all operations
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security ??= [];
                operation.Value.Security.Add(securityRequirements);
            }
        }
    }
}
