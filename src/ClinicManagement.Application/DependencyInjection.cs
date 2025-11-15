
using ClinicManagement.Application.Common.Behaviors;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Options;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Application;
public  static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.Configure<JwtOption>(configuration.GetSection("Jwt"));
        services.Configure<SmtpOptions>(configuration.GetSection("Email"));

        return services;
    }
}
