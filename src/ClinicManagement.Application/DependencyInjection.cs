using ClinicManagement.Application.Common.Behaviors;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Options;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });
        
        services.AddMapster();
        MappingConfig.RegisterMappings();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        // Keep only validation behavior - simple and essential
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        return services;
    }
}