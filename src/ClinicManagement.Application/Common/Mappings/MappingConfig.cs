using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ClinicManagement.Application.Common.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
    }
}
