# ============================================================
# STAGE 1: Build
# Uses the full .NET SDK image to restore, build, and publish.
# This image is large (~800MB) but only used during build time.
# The final image won't include it.
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and all .csproj files first.
# Docker caches each layer - if these files don't change,
# the 'dotnet restore' layer is reused (faster rebuilds).
COPY ClinicManagement.sln .
COPY src/ClinicManagement.API/ClinicManagement.API.csproj                         src/ClinicManagement.API/
COPY src/ClinicManagement.Application/ClinicManagement.Application.csproj         src/ClinicManagement.Application/
COPY src/ClinicManagement.Domain/ClinicManagement.Domain.csproj                   src/ClinicManagement.Domain/
COPY src/ClinicManagement.Infrastructure/ClinicManagement.Infrastructure.csproj   src/ClinicManagement.Infrastructure/

# Restore NuGet packages (cached unless .csproj files change)
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Publish in Release mode to /app/publish
# --no-restore: skip restore since we already did it above
RUN dotnet publish src/ClinicManagement.API/ClinicManagement.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================================
# STAGE 2: Runtime
# Uses the smaller ASP.NET runtime image (~200MB).
# Only contains what's needed to RUN the app, not build it.
# This is the final image that gets deployed.
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user for security.
# Running as root inside a container is a security risk.
RUN adduser --disabled-password --gecos "" appuser

# Create the logs directory and give ownership to appuser
RUN mkdir -p /app/Logs && chown -R appuser:appuser /app

# Copy only the published output from the build stage
COPY --from=build --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Expose port 8080 (ASP.NET Core default in containers)
EXPOSE 8080

# Environment variables that configure ASP.NET Core for containers.
# ASPNETCORE_URLS: listen on all interfaces on port 8080
# ASPNETCORE_ENVIRONMENT: set to Production (overridable at runtime)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Start the application
ENTRYPOINT ["dotnet", "ClinicManagement.API.dll"]
