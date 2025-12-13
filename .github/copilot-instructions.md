# Copilot Instructions for Cloudbb .NET Service

## Project Overview

Full-stack ASP.NET Core 10.0 Web API with Blazor WebAssembly frontend, using Identity authentication, JWT tokens, PostgreSQL database via Entity Framework Core, and GitLab CI/CD. The project follows strict coding standards emphasizing explicit typing and readability.

## Architecture & Structure

### Core Components

- **`source/Cloudbb/Cloudbb.Web/`** - Main web API project with versioned controllers (`Api/V1/Controllers/`)
- **`source/Cloudbb/Cloudbb.Client/`** - Blazor WebAssembly client with MudBlazor components
- **`Data/ApplicationDbContext.cs`** - EF Core context with Identity integration, enforces explicit entity/property mapping policies
- **`Data/Model/`** - Domain entities (CloudbbUser extends Identity, CloudbbPost/Comment with voting system)
- **`Services/Auth/`** - Interface-driven JWT authentication services with ECDSA signing
- **`Configuration/`** - API versioning, Swagger configuration, CORS for Blazor client
- **`source/Cloudbb/Cloudbb.Web.Tests/`** - Unit tests using MSTest v4 with Moq
- **`source/Cloudbb/Cloudbb.Web.Tests.Integration/`** - Integration tests with PostgreSQL test database
- **`/docs/api-v1.json`** - OpenAPI specification for client implementation

### Technology Stack

- **.NET 10.0** with nullable reference types enabled globally
- **Blazor WebAssembly** client with MudBlazor UI components, JWT authentication
- **PostgreSQL** via Npgsql.EntityFrameworkCore.PostgreSQL with source-generated model discovery
- **ASP.NET Core Identity** with JWT Bearer authentication (zero clock skew)
- **Entity Framework Core** with auto-apply migrations on startup
- **MSTest.Sdk/4.0.1** for testing with Moq 4.20.72
- **API Versioning** with grouped Swagger docs in Development
- **CORS** configured for Blazor client cross-origin requests
- **GitLab CI/CD** with staged builds, unit tests, and integration tests

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
dotnet test source/Cloudbb/Cloudbb.Web.Tests.Integration
```

## Service Patterns

### Interface-Driven Design

All business services implement interfaces for testability:

```csharp
// Service registration pattern in Program.cs
builder.Services.AddSingleton<IJwtAlgorithmProvider, JwtEcdsaSha256AlgorithmProvider>();
builder.Services.AddSingleton<IJwtECDsaSigningKeyImportService, JwtECDsaPemFileSigningKeyImportService>();
builder.Services.AddSingleton<IJwtSigningKeyProvider, JwtECDsaSigningKeyProvider>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Interface definition pattern
public interface IJwtService
{
    ValueTask<IJwtToken> GenerateTokenAsync(CloudbbUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
```

### Authentication Architecture

- **JWT with ECDSA signing** (ES256) for production security via PEM key files
- **Zero clock skew** validation: `ClockSkew = TimeSpan.Zero`
- **Claims-based** with `NameIdentifier`, `Name`, `Email`, `Role`, `Jti`, `Iat`
- **Global authorization** with `[Authorize]` on controllers
- **CORS integration** for Blazor client (`BlazorClient` policy for ports 7089/5097)

## Client Development

### Blazor WebAssembly Client

- **Project**: `source/Cloudbb/Cloudbb.Client/` - Blazor WASM with MudBlazor
- **API Integration**: Use `/docs/api-v1.json` for client model definitions and contracts
- **Authentication**: JWT stored in localStorage, `JwtAuthenticationStateProvider` for auth state
- **Services**: `AuthService` for login/register, `HttpClient` with Bearer token injection

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

## Integration Testing

### PostgreSQL Test Database

- **Integration tests**: `source/Cloudbb/Cloudbb.Web.Tests.Integration/`
- **Test database**: Separate PostgreSQL instance for integration tests
- **GitLab CI**: Uses `postgres:18-trixie` service container
- **Database initialization**: `IntegrationTestDbInitializer` and `DatabaseInitializer`
- **Component testing**: `TransactionalControllerTest` base class for end-to-end scenarios

### Integration Test Patterns

**CRITICAL**: Integration tests use `Wkg.AspNetCore.TestAdapters` which provides automatic transaction rollback for each test scope, regardless of whether tested services attempt to commit transactions.

#### Controller Integration Tests

Follow this established pattern for controller tests:

```csharp
[TestClass]
public sealed class ControllerName_ActionTests : ControllerBaseTest<ControllerName>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(ControllerName component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    // Passing the request through UsingComponentAsync enables automatic model validation for the request object.
    // (sets up ModelState for the subsequent controller action call)
    public Task ActionAsync_WithCondition_ExpectedBehaviorAsync() => UsingComponentAsync(new RequestType()
    {
        Property = "TestValue"
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.ActionAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        ResponseType response = Assert.IsInstanceOfType<ResponseType>(ok.Value);
        
        // Verify database changes if needed
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
        SomeEntity? entity = await dbContext.Set<SomeEntity>().FindAsync(response.Id, ct);
        Assert.IsNotNull(entity);
    }, TestContext.CancellationToken);
}
```

#### Advanced Integration Test Patterns

**Multi-step transactions** use `UsingTransactionAsync` for operations spanning multiple component calls. The transaction is bound to the test scope and spans across multiple `UsingComponentAsync` calls. Any changes are rolled back at the end of the test.

```csharp
[TestMethod]
public Task ComplexFlow_WithMultipleOperations_ShouldWorkAsync() => UsingTransactionAsync(async (dbContext, ct) =>
{
    // Arrange initial data state with the dbContext if needed
    // dbContext.Add(...);
    await dbContext.SaveChangesAsync(ct);

    // optional: call into UsingServiceProviderAsync() to set up any additional data if dbContext is not sufficient.
    // note: the serviceProvider here shares the same transaction scope as the dbContext (from the transaction)
    // UsingTransactionAsync internally also uses UsingServiceProviderAsync to provide the serviceProvider. Nested calls share the same scope.
    await UsingServiceProviderAsync(async (serviceProvider, ct1) =>
    {
        // e.g. get UserManager to create users
        UserManager<ApplicationDbContext> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationDbContext>>();
        // ... set up data ...
    }, ct);

    // First operation
    await UsingComponentAsync(new FirstRequest(), async (controller, request, ct1) =>
    {
        IActionResult result = await controller.FirstActionAsync(request, ct1);
        Assert.IsInstanceOfType<OkObjectResult>(result);
    }, ct);

    // Second operation in same transaction, but with a new controller instance (to simulate separate API call)
    await UsingComponentAsync(new SecondRequest(), async (controller, request, ct1) =>
    {
        IActionResult result = await controller.SecondActionAsync(request, ct1);
        Assert.IsInstanceOfType<OkObjectResult>(result);
    }, ct);
}, TestContext.CancellationToken);
```

**Component tests without request objects** for simple operations:

```csharp
// could also obviously be combined with more complex setup through UsingTransactionAsync as above
[TestMethod]
public Task SimpleOperation_ShouldWork() => UsingComponentAsync(async (controller, serviceProvider, ct) =>
{
    IActionResult result = await controller.SimpleActionAsync(ct);
    Assert.IsInstanceOfType<OkResult>(result);
}, TestContext.CancellationToken);
```

#### Key Integration Test Guidelines

- **Inherit from**: `ControllerBaseTest<TController>` for API controllers
- **Authentication**: Use `SetAuthenticatedUserContextAsync(controller, serviceProvider, testUser, ct)` to set user context
- **File naming**: `ControllerName_ActionTests.cs` for controller actions
- **Test data**: Use `IntegrationTestDbLoader` static properties for pre-seeded test data
- **Async pattern**: Always return `Task` from test methods, use `TestContext.CancellationToken`
- **Database isolation**: Each test runs in its own transaction scope that auto-rolls back
- **Service access**: Use `serviceProvider.GetRequiredService<T>()` to access registered services
- **Validation**: Use `UsingComponentAsync(request, ...)` to enable automatic model validation

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

## CI/CD Pipeline

### GitLab CI Stages
- **Build**: `dotnet build` with dependency caching
- **Unit Tests**: `dotnet test Cloudbb.Web.Tests` 
- **Integration Tests**: `dotnet test Cloudbb.Web.Tests.Integration` with PostgreSQL service
- **Caching**: NuGet packages and build artifacts cached per stage/branch
- **Environment**: Uses `mcr.microsoft.com/dotnet/sdk:10.0-alpine` image

When adding features, maintain strict interface-driven design, follow the explicit typing rules, register all services in `Program.cs` with appropriate lifetimes, and write comprehensive MSTest v4 tests with mocked dependencies.

## Client-Server Integration

### API Contract Reference

- **Always use `/docs/api-v1.json`** as the definitive API contract when implementing client functionality
- **Authentication**: Use JWT Bearer tokens with proper CORS configuration (`BlazorClient` policy)
- **Error handling**: API returns structured error responses with validation details and trace IDs