using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

app.UseAppConfigurations();

await app.RunAsync();