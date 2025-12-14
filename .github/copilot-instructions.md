# Copilot Instructions for Cloudbb .NET Service

## Project Overview

ASP.NET Core 10.0 Web API with Identity authentication, JWT tokens, PostgreSQL database via Entity Framework Core, and GitLab CI/CD. The project follows strict coding standards emphasizing explicit typing and readability.

## Architecture & Structure

### Core Components

- **`source/Cloudbb/Cloudbb.Web/`** - Main web API project
- **`Data/ApplicationDbContext.cs`** - EF Core context with Identity integration, uses PostgreSQL with custom schema (`identity`)
- **`Services/`** - Business logic layer (JWT, database seeding)
- **`Controllers/`** - API endpoints (Auth controller for Identity operations)
- **`Models/`** - DTOs and request/response models

### Technology Stack

- **.NET 10.0** with nullable reference types enabled globally
- **PostgreSQL** via Npgsql.EntityFrameworkCore.PostgreSQL
- **ASP.NET Core Identity** with JWT Bearer authentication
- **Entity Framework Core** with migrations
- **GitLab CI/CD** with Docker deployment

## Critical Coding Standards

Always source `/code-style.md` before generating code to ensure compliance with project conventions. Key standards include:

### Naming & Visibility

- **Never use `var`** - always explicit types (prevents async bugs)
- **Primary constructor parameters** in `camelCase`, private fields `_camelCase`
- **All types sealed** unless inheritance required
- **Visibility always explicit** (`private`, `public`, etc.) even when default

### Code Structure

- **File-scoped namespaces** (`namespace Cloudbb.Web;`)
- **Allman bracing** with exceptions for properties/initializers
- **Async methods** suffixed with `Async`
- **Target-typed new** when type explicit on left side: `FileStream stream = new(...);`

### Authentication Patterns

```csharp
// JWT service injection pattern
builder.Services.AddScoped<IJwtService, JwtService>();

// Controller auth pattern
[Authorize]
[HttpGet("profile")]
public async Task<IActionResult> GetProfile()
```

## Development Workflows

### Database Operations

```bash
# From source/Cloudbb/Cloudbb.Web/ directory
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Build & Test

```bash
# GitLab CI uses these commands
dotnet restore --packages .nuget
dotnet build --no-restore
dotnet test --no-restore
```

### Project Structure Commands

- Build from solution root: `dotnet build source/Cloudbb/Cloudbb.slnx`
- Run project: `dotnet run --project source/Cloudbb/Cloudbb.Web`

## Configuration Patterns

### Service Registration

- **Scoped services** for business logic (`IJwtService`, `IDatabaseSeedService`)
- **DbContext** with typed options: `AddDbContext<ApplicationDbContext>`
- **Identity configuration** with explicit password/lockout policies

### Connection Strings

- **Development**: `cloudbb_dev` database
- **Production**: `cloudbb` database
- **JWT settings** in appsettings with issuer/audience/key

## Key Files for Context

### Essential Architecture Files

- **`Program.cs`** - Service registration, middleware pipeline, authentication setup
- **`Data/ApplicationDbContext.cs`** - Database schema, Identity table configuration
- **`Services/JwtService.cs`** - Token generation/validation patterns

### Configuration Files

- **`appsettings.json`** - Production configuration with PostgreSQL connection
- **`appsettings.Development.json`** - Dev-specific settings
- **`.gitlab-ci.yml`** - CI/CD pipeline with .NET 10.0 Alpine container

## Project-Specific Patterns

### Identity Integration

- **Custom table schema**: All Identity tables in `identity` schema
- **Sealed ApplicationDbContext** with typed options
- **Role-based authorization** with Admin/User/Manager roles

### Error Prevention

- **No magic strings** - use `nameof()` and constants
- **Named parameters** for clarity: `CompressionMode.Compress, leaveOpen: true`
- **Explicit async/await** - never use `var` with async methods

When adding features, maintain consistency with existing patterns, follow the strict typing rules, and ensure all new services are properly registered in `Program.cs`.