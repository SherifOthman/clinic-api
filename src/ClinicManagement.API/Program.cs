using ClinicManagement.API;
using ClinicManagement.API.Middleware;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.Data;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
