# Copilot Instructions for Cloudbb .NET Service

## Project Overview

ASP.NET Core 10.0 Web API with Identity authentication, JWT tokens, PostgreSQL database via Entity Framework Core, and GitLab CI/CD. The project follows strict coding standards emphasizing explicit typing and readability.

## Architecture & Structure

### Core Components

- **`source/Cloudbb/Cloudbb.Web/`** - Main web API project
- **`Data/ApplicationDbContext.cs`** - EF Core context with Identity integration, uses PostgreSQL with custom schema (`identity`)
- **`Data/Model/`** - Domain entities (CloudbbUser, CloudbbPost, CloudbbComment with voting system)
- **`Services/Auth/Default/`** - JWT authentication services with interface-driven design
- **`Controllers/`** - API endpoints (Auth controller for Identity operations)
- **`Api/Models/`** - DTOs and request/response models

### Technology Stack

- **.NET 10.0** with nullable reference types enabled globally
- **PostgreSQL** via Npgsql.EntityFrameworkCore.PostgreSQL
- **ASP.NET Core Identity** with JWT Bearer authentication
- **Entity Framework Core** with migrations
- **MSTest v4** for testing with Moq 4.20.72
- **GitLab CI/CD** with Docker deployment using Kaniko

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
- **Internal services** exposed to tests via `_friends.cs` with `InternalsVisibleTo("Cloudbb.Web.Tests")`

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
# Use custom migration script from Cloudbb.Web directory
./add-migration.sh MigrationName  # Validates PascalCase, outputs to Data/Migrations
dotnet ef database update
```

### Build & Test

```bash
# GitLab CI uses these exact commands
dotnet restore source/Cloudbb --packages .nuget
dotnet build source/Cloudbb --no-restore
dotnet test source/Cloudbb --no-restore
```

### Project Structure Commands

- Build from solution root: `dotnet build source/Cloudbb/Cloudbb.slnx`
- Run project: `dotnet run --project source/Cloudbb/Cloudbb.Web`
- Add migrations: `cd source/Cloudbb/Cloudbb.Web && ./add-migration.sh MigrationName`

## Testing Patterns

### MSTest v4 Framework

**CRITICAL**: Before writing tests, AI agents must first fetch https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-migration-v3-v4 to avoid using deprecated APIs.

Follow existing test patterns in `Cloudbb.Web.Tests/Services/Auth/Default/`:

```csharp
[TestClass]
public sealed class ServiceNameTests
{
    private readonly Mock<IDependency> _mockDependency = new();
    private readonly ServiceName _service;
    
    public TestContext TestContext { get; set; }  // Required for MSTest v4
    
    public ServiceNameTests() 
    {
        _service = new ServiceName(_mockDependency.Object);
    }
    
    [TestMethod]
    public async Task MethodName_WithCondition_ExpectedBehaviorAsync()
    {
        // Arrange
        _mockDependency.Setup(x => x.Method()).Returns(value);
        
        // Act
        ResultType result = await _service.MethodAsync(TestContext.CancellationToken);
        
        // Assert
        Assert.IsNotNull(result);
        // Use MSTest v4 assertion methods: Assert.ThrowsExactlyAsync, Assert.HasCount, etc.
    }
}
```

### Test Configuration

- **Test project uses MSTest.Sdk/4.0.2** - modern MSTest with simplified project structure
- **Moq 4.20.72** for mocking with `Mock<T>.Setup()` patterns
- **Internal access** via `InternalsVisibleTo` allows testing internal services directly
- **Isolation testing** - each service tested in isolation with all dependencies mocked

## Configuration Patterns

### Service Registration

- **Interface-driven design**: All services implement interfaces for testability
- **Scoped lifetimes** for business logic (`IJwtService`, `IDatabaseSeedService`)
- **DbContext** with typed options: `AddDbContext<ApplicationDbContext>`
- **Identity configuration** with explicit password/lockout policies

### CI/CD Integration

- **Version injection**: GitLab CI injects version into both `csproj` and `CloudbbWeb.cs` runtime constants
- **Multi-stage builds**: Build → Test → Deploy with Docker containerization
- **Alpine-based images**: Uses `mcr.microsoft.com/dotnet/sdk:10.0-alpine` for smaller footprint

## Key Files for Context

### Essential Architecture Files

- **`Program.cs`** - Service registration, middleware pipeline, authentication setup
- **`Data/ApplicationDbContext.cs`** - Database schema with Identity integration
- **`_friends.cs`** - Assembly-level InternalsVisibleTo configuration
- **`Services/Auth/Default/`** - JWT authentication implementation with provider pattern
- **`Data/Model/CloudbbUser.cs`** - Domain entity extending Identity with custom properties

### Configuration Files

- **`appsettings.json`** - Production PostgreSQL connection (`cloudbb` database)
- **`appsettings.Development.json`** - Development settings (`cloudbb_dev` database)
- **`.gitlab-ci.yml`** - CI/CD with .NET 10.0 Alpine, caching, and Kaniko deployment
- **`add-migration.sh`** - Custom migration script with validation

## Project-Specific Patterns

### Domain Model

- **CloudbbUser extends Identity**: Custom domain properties while leveraging ASP.NET Core Identity
- **Vote-based system**: CloudbbPost, CloudbbComment with CloudbbVote entities for user interactions
- **Connection entities**: ICloudbbConnectionEntity for entity relationships

### Error Prevention

- **No magic strings** - use `nameof()` and constants
- **Explicit null handling**: Use nullable reference types and avoid using the null-forgiving operator (`!`), unless absolutely necessary
- **Named parameters** for clarity: `CompressionMode.Compress, leaveOpen: true`
- **Global suppressions**: Configured in `GlobalSuppressions.cs` for EF migrations and API controllers

When adding features, maintain consistency with existing patterns, follow the strict typing rules, ensure all new services implement interfaces and are properly registered in `Program.cs`, and write comprehensive tests following the MSTest v4 patterns.