# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution file and project files for better layer caching
COPY QuickBiteAiApp.sln ./
COPY QuickBite.Api/QuickBite.Api.csproj ./QuickBite.Api/
COPY QuickBite.Api.Tests/QuickBite.Api.Tests.csproj ./QuickBite.Api.Tests/

# Restore dependencies
RUN dotnet restore QuickBiteAiApp.sln

# Copy the rest of the source code
COPY . ./

# Build and publish the application in Release mode
RUN dotnet publish QuickBite.Api/QuickBite.Api.csproj -c Release -o out --no-restore

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' --shell /bin/bash --uid 1001 appuser

# Copy the published application from the build stage
COPY --from=build-env /app/out .

# Create directory for SQLite database and set permissions
RUN mkdir -p /app/data && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 5026
EXPOSE 5026

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5026
ENV ASPNETCORE_ENVIRONMENT=Production

# Configure the entry point
ENTRYPOINT ["dotnet", "QuickBite.Api.dll"]