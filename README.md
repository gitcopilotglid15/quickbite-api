# QuickBite API

A modern .NET 8 Web API for managing restaurant menu items with full CRUD operations, built using Entity Framework Core with SQLite database.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Building the Application](#building-the-application)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [API Documentation](#api-documentation)
- [Docker Support](#docker-support)
- [Development Workflow](#development-workflow)
- [Database Information](#database-information)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

## Overview

QuickBite API is a comprehensive restaurant menu management system that provides:

- **CRUD Operations**: Create, Read, Update, and Delete menu items
- **Rich Data Model**: Support for categories, dietary tags, ingredients, and pricing
- **Comprehensive Documentation**: XML documentation and Swagger/OpenAPI integration
- **Unit Testing**: Extensive test coverage with xUnit, Moq, and FluentAssertions
- **Database Management**: SQLite with Entity Framework Core and automatic migrations
- **Containerization**: Docker support for easy deployment

## Prerequisites

### Required
- **.NET 8.0 SDK** or later
- **Git** for version control

### Optional
- **Visual Studio 2022** (17.8 or later) or **Visual Studio Code**
- **Docker Desktop** (for containerized deployment)
- **Postman** or similar API testing tool

### Verify Prerequisites

```bash
# Check .NET version
dotnet --version

# Should return 8.0.x or later
```

## Project Structure

```
QuickBiteAiApp/
├── QuickBite.Api/                 # Main API project
│   ├── Controllers/               # API controllers
│   ├── Data/                     # Database context and configurations
│   ├── Models/                   # Domain models
│   ├── Extensions/               # Service extensions
│   └── Program.cs                # Application entry point
├── QuickBite.Api.Tests/          # Unit test project
│   ├── Controllers/              # Controller tests
│   └── UnitTest1.cs             # Basic test file
├── QuickBiteAiApp.sln           # Solution file
├── Dockerfile                    # Docker configuration
├── docker-compose.yml           # Docker Compose configuration
└── README.md                     # This file
```

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd QuickBite/quickbite-ai-app
```

### 2. Restore Dependencies

```bash
# Restore NuGet packages for the entire solution
dotnet restore
```

### 3. Verify Installation

```bash
# Build the solution to ensure everything is set up correctly
dotnet build
```

## Building the Application

### Build All Projects

```bash
# Build the entire solution
dotnet build

# Build in Release mode
dotnet build --configuration Release
```

### Build Specific Projects

```bash
# Build only the API project
dotnet build QuickBite.Api/QuickBite.Api.csproj

# Build only the test project
dotnet build QuickBite.Api.Tests/QuickBite.Api.Tests.csproj
```

### Clean Build

```bash
# Clean previous build artifacts
dotnet clean

# Clean and rebuild
dotnet clean && dotnet build
```

## Running the Application

### Method 1: Using .NET CLI (Recommended for Development)

```bash
# Run the API project
dotnet run --project QuickBite.Api

# Run with specific profile
dotnet run --project QuickBite.Api --launch-profile https
```

The application will start and be available at:
- **HTTP**: `http://localhost:5025`
- **HTTPS**: `https://localhost:7026`

### Method 2: Using Visual Studio

1. Open `QuickBiteAiApp.sln` in Visual Studio
2. Set `QuickBite.Api` as the startup project
3. Press **F5** or click **Debug > Start Debugging**

### Method 3: Using Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# Access the application at http://localhost:5026
```

### Application URLs

Once running, you can access:
- **API Base**: `http://localhost:5025/api`
- **Swagger UI**: `http://localhost:5025/swagger`
- **OpenAPI Spec**: `http://localhost:5025/swagger/v1/swagger.json`

## Testing

### Run All Tests

```bash
# Run all tests in the solution
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Tests

```bash
# Run tests from a specific project
dotnet test QuickBite.Api.Tests/QuickBite.Api.Tests.csproj

# Run tests matching a pattern
dotnet test --filter "MenuItemController"

# Run a specific test method
dotnet test --filter "TestCreateMenuItem_ShouldReturnSavedMenuItem"
```

### Test Categories

The test suite includes:
- **Unit Tests**: Testing individual components in isolation
- **Integration Tests**: Testing API endpoints with in-memory database
- **Controller Tests**: Testing HTTP controllers with mocked dependencies

### Test Frameworks Used

- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Expressive assertion library
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support

## API Documentation

### Swagger UI

When running in Development mode, Swagger UI is available at:
- `http://localhost:5025/swagger`

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/menuitem` | Get all menu items |
| GET | `/api/menuitem/{id}` | Get menu item by ID |
| POST | `/api/menuitem` | Create new menu item |
| PUT | `/api/menuitem/{id}` | Update existing menu item |
| DELETE | `/api/menuitem/{id}` | Delete menu item |

### Sample API Usage

```bash
# Get all menu items
curl -X GET "http://localhost:5025/api/menuitem"

# Create a new menu item
curl -X POST "http://localhost:5025/api/menuitem" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Margherita Pizza",
    "description": "Classic pizza with tomatoes and mozzarella",
    "price": 12.99,
    "category": "mains",
    "dietaryTags": ["vegetarian"],
    "ingredients": ["pizza dough", "tomatoes", "mozzarella"]
  }'
```

## Docker Support

The application includes full Docker support for containerized deployment.

### Quick Start with Docker

```bash
# Build and run with Docker Compose
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop the application
docker-compose down
```

### Manual Docker Build

```bash
# Build the Docker image
docker build -t quickbite-api .

# Run the container
docker run -p 5026:5026 quickbite-api
```

For detailed Docker instructions, see [README-Docker.md](README-Docker.md).

## Development Workflow

### 1. Daily Development

```bash
# Pull latest changes
git pull

# Restore packages (if needed)
dotnet restore

# Run the application
dotnet run --project QuickBite.Api

# Run tests
dotnet test
```

### 2. Making Changes

```bash
# Create a feature branch
git checkout -b feature/your-feature-name

# Make your changes...

# Run tests to ensure nothing breaks
dotnet test

# Build to check compilation
dotnet build

# Commit your changes
git add .
git commit -m "Add your feature description"
```

### 3. Database Changes

```bash
# Add a new migration (if you modify models)
dotnet ef migrations add YourMigrationName --project QuickBite.Api

# Update the database
dotnet ef database update --project QuickBite.Api
```

## Database Information

### Technology Stack
- **Database**: SQLite (file-based, zero-configuration)
- **ORM**: Entity Framework Core 9.0.9
- **Migrations**: Automatic database creation and seeding

### Database Location
- **Development**: `QuickBite.Api/quickbite.db`
- **Docker**: Persisted in Docker volume

### Sample Data
The application automatically seeds the database with sample menu items on first run.

### Database Operations

```bash
# Check migration status
dotnet ef migrations list --project QuickBite.Api

# Generate SQL script for migrations
dotnet ef migrations script --project QuickBite.Api

# Reset database (development only)
# Delete the .db file and restart the application
```

## Troubleshooting

### Common Issues

#### Port Already in Use
```bash
# Check what's using port 5025
netstat -ano | findstr :5025

# Kill the process (Windows)
taskkill /PID <PID> /F
```

#### Database Issues
```bash
# Reset the database
rm QuickBite.Api/quickbite.db
dotnet run --project QuickBite.Api
```

#### Build Errors
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

#### Test Failures
```bash
# Run tests with detailed output
dotnet test --verbosity diagnostic

# Run tests one at a time
dotnet test --logger "console;verbosity=detailed"
```

### Performance Tips

- Use `dotnet run` for development (faster startup)
- Use `dotnet build --configuration Release` for production builds
- Enable file watching: `dotnet watch run --project QuickBite.Api`

### Getting Help

1. Check this README for common solutions
2. Review the application logs
3. Check the GitHub Issues section
4. Verify your .NET version matches requirements

## Contributing

### Code Standards
- Follow C# coding conventions
- Add XML documentation for public APIs
- Write unit tests for new features
- Use meaningful commit messages

### Pull Request Process
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Ensure all tests pass
6. Submit a pull request

### Development Setup
```bash
# Enable file watching for development
dotnet watch run --project QuickBite.Api

# Run tests in watch mode
dotnet watch test --project QuickBite.Api.Tests
```

---

## Quick Command Reference

```bash
# Essential commands
dotnet restore                          # Restore dependencies
dotnet build                           # Build the solution
dotnet run --project QuickBite.Api     # Run the API
dotnet test                            # Run all tests

# Docker commands
docker-compose up --build              # Build and run with Docker
docker-compose logs -f                 # View logs
docker-compose down                    # Stop containers

# Database commands
dotnet ef migrations add <Name>        # Add migration
dotnet ef database update              # Update database

For questions or support, please refer to the project documentation or create an issue in the repository.


## One thing Copilot helped you achieve faster
    Copilot helpe me to create basic application structure, installing plugins and dependencies. 
## One time you had to reject or refactor Copilot’s code
    Unsecure code generated for add menu.
