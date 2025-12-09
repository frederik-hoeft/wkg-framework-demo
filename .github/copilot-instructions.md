# Copilot Instructions for Cloudbb .NET Service

## Project Overview

ASP.NET Core 10.0 Web API with Identity authentication, JWT tokens, PostgreSQL database via Entity Framework Core, and GitLab CI/CD. The project follows strict coding standards emphasizing explicit typing and readability.

## Architecture & Structure

### Core Components

- **`source/Cloudbb/Cloudbb.Web/`** - Main web API project with versioned controllers (`Api/V1/Controllers/`)
- **`Data/ApplicationDbContext.cs`** - EF Core context with Identity integration, enforces explicit entity/property mapping policies
- **`Data/Model/`** - Domain entities (CloudbbUser extends Identity, CloudbbPost/Comment with voting system)
- **`Services/Auth/`** - Interface-driven JWT authentication services with ECDSA signing
- **`Configuration/`** - API versioning, Swagger configuration with grouped endpoints

### Technology Stack

- **.NET 10.0** with nullable reference types enabled globally
- **PostgreSQL** via Npgsql.EntityFrameworkCore.PostgreSQL with source-generated model discovery
- **ASP.NET Core Identity** with JWT Bearer authentication (zero clock skew)
- **Entity Framework Core** with auto-apply migrations on startup
- **MSTest.Sdk/4.0.2** for testing with Moq 4.20.72
- **API Versioning** with grouped Swagger docs in Development

## Critical Coding Standards

**MANDATORY**: Always read `/code-style.md` before generating code. Key non-negotiable rules:

### Naming & Visibility

- **Never use `var`** - always explicit types for clarity
- **Primary constructor parameters** in `camelCase`, private fields `_camelCase`
- **All types sealed** unless inheritance required
- **Visibility always explicit** (`private`, `public`, etc.) even when default

### Code Structure

- **File-scoped namespaces** (`namespace Cloudbb.Web;`)
- **Allman bracing** with exceptions for auto-properties/initializers
- **Async methods** suffixed with `Async`
- **Target-typed new** when type explicit on left side: `FileStream stream = new(...);`

### Error Prevention

- **No magic strings/numbers** - use `nameof()` and constants
- **Named parameters** for clarity: `CompressionMode.Compress, leaveOpen: true`
- **Explicit null handling** - nullable reference types, avoid `!` operator unless absolutely necessary

## Development Workflows

### Database Operations

```bash
# ALWAYS use custom migration script from Cloudbb.Web directory
cd source/Cloudbb/Cloudbb.Web
./add-migration.sh MigrationName  # Validates PascalCase, outputs to Data/Migrations
dotnet ef database update
```

### Build & Test

```bash
# Standard commands from solution root
dotnet build source/Cloudbb/Cloudbb.slnx
dotnet run --project source/Cloudbb/Cloudbb.Web
dotnet test source/Cloudbb/Cloudbb.Web.Tests
```

## Service Patterns

### Interface-Driven Design

All business services implement interfaces for testability:

```csharp
// Service registration pattern in Program.cs
builder.Services.AddSingleton<IJwtAlgorithmProvider, JwtEcdsaSha256AlgorithmProvider>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Interface definition pattern
public interface IJwtService
{
    ValueTask<IJwtToken> GenerateTokenAsync(CloudbbUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
```

### Authentication Architecture

- **JWT with ECDSA signing** (not HMAC) for production security
- **Zero clock skew** validation: `ClockSkew = TimeSpan.Zero`
- **Claims-based** with `NameIdentifier`, `Name`, `Email`, `Role`, `Jti`, `Iat`
- **Global authorization** with `[Authorize]` on controllers

## Testing Patterns

### MSTest v4 Framework

**CRITICAL**: Before writing tests, AI agents must first fetch https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-migration-v3-v4 to avoid using deprecated APIs.

Follow existing test patterns:

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

### Internal Access Pattern

- **`_friends.cs`**: `[assembly: InternalsVisibleTo("Cloudbb.Web.Tests")]`
- **Test internal services directly** without exposing them publicly
- **Mock all dependencies** for isolation testing

## EF Core Patterns

### Explicit Mapping Requirements

```csharp
// ApplicationDbContext enforces explicit policies
builder.LoadModels(modelLoader, modelOptions => modelOptions
    .ConfigurePolicies(policies => policies
        .AddPolicy<EntityNaming>(naming => naming.RequireExplicit())
        .AddPolicy<PropertyMapping>(mapping => mapping.RequireExplicit())
        .AddPolicy<EntityInheritanceValidation>(entity => entity
            .MustExtend<CloudbbEntity>()
            .UnlessExtends<ICloudbbConnectionEntity>())));
```

### Domain Model Patterns

- **CloudbbUser extends CloudbbEntity**: Bridge between Identity and domain
- **Vote entities**: CloudbbPostVote, CloudbbCommentVote for user interactions
- **Connection entities**: Implement `ICloudbbConnectionEntity` for many-to-many relationships

## Key Files for Context

### Essential Reading
- **`Program.cs`** - Service registration, JWT config, middleware pipeline
- **`code-style.md`** - Complete coding standards (MUST READ before coding)
- **`Data/ApplicationDbContext.cs`** - EF Core policies and model loading
- **`add-migration.sh`** - Custom migration script with PascalCase validation
- **`_friends.cs`** - Test access configuration

### Configuration Structure
- **API versioning**: Default v1.0 with URL substitution (`api/v{version}/`)
- **Swagger**: Development-only with Bearer auth, grouped by version
- **Connection strings**: `DatabaseConnection` for PostgreSQL (`cloudbb`/`cloudbb_dev`)
- **JWT settings**: `Auth:Jwt:*` configuration keys

When adding features, maintain strict interface-driven design, follow the explicit typing rules, register all services in `Program.cs` with appropriate lifetimes, and write comprehensive MSTest v4 tests with mocked dependencies.