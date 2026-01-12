# Cloudbb .NET Service

Cloudbb is a feature-complete ASP.NET Core 10.0 forum Web API using PostgreSQL, ASP.NET Core Identity, and JWT authentication. The project provides full forum functionality including user authentication, post management, comment systems, and community voting features with comprehensive testing coverage.

## Overview
- **Core app**: `source/Cloudbb/Cloudbb.Web` (`net10.0`, nullable enabled)
- **Blazor Client**: `source/Cloudbb/Cloudbb.Client` - WebAssembly SPA with MudBlazor UI
- **Auth**: ASP.NET Core Identity + JWT (RSA RS256 asymmetric signing), global authorization
- **Data**: PostgreSQL via EF Core with explicit entity naming/mapping and model discovery
- **API Versioning**: `Asp.Versioning` with grouped Swagger in Development  
- **Transactions**: `ReadCommitted` via `Wkg.AspNetCore.Transactions`
- **Testing**: Comprehensive unit and integration test suites using MSTest v4

## Features
- **User Authentication**: JWT-based registration, login with secure RSA signing
- **Forum Posts**: Create, edit, delete posts with revision history and community voting
- **Comment System**: Threaded comments on posts with voting and moderation
- **Community Voting**: Upvote/downvote system for posts and comments
- **User Management**: Role-based authorization (admin/user) with Identity integration
- **API Documentation**: Comprehensive Swagger/OpenAPI docs with examples
- **Modern UI**: Blazor WebAssembly SPA with MudBlazor components
  - Responsive design with pagination and sorting
  - Real-time timezone preference (stored in browser)
  - Interactive voting on posts and comments
  - Inline editing and deletion for owned content

## Project Structure
- `source/Cloudbb/Cloudbb.Web` - Main Web API project
    - `Api/V1/Controllers/`: versioned controllers split into `.api.cs` (routing/docs) and `.cs` (logic)
        - `AuthController`: user registration, login, JWT token management
        - `PostsController`: forum post CRUD operations, voting, retrieval with pagination
        - `CommentsController`: comment creation, deletion, voting on posts
    - `Configuration/Swagger/`: Swagger configuration and filters
    - `Data/`: DbContext, model loader, entities (posts, comments, votes, users), migrations
    - `Services/Auth/`: JWT services with RSA key management and claim handling
- `source/Cloudbb/Cloudbb.Client` - Blazor WebAssembly client
    - `Pages/`: Razor pages (Home, Login, Register, Posts, PostDetail, CreatePost, EditPost)
    - `Components/`: Reusable components (TimeZoneSelector)
    - `Services/`: API client services (Auth, Posts, Comments, TimeZone)
    - `Models/`: DTOs matching API specification
    - `Layout/`: App layout and navigation
- `source/Cloudbb/Cloudbb.Web.Tests` - Unit tests (MSTest v4)
    - `Services/Auth/`: JWT service, key import, algorithm provider tests
- `source/Cloudbb/Cloudbb.Web.Tests.Integration` - Integration tests
    - `Api/V1/Auth/`, `Api/V1/Posts/`, `Api/V1/Comments/`: comprehensive controller testing

## Conventions
See `code-style.md`. Highlights:
- No `var`; use explicit types. Async methods end with `Async`.
- File-scoped namespaces; Allman bracing; explicit visibility; sealed internal types.
- Target-typed `new` only when the left-hand type is explicit.

## Configuration
Set these in `appsettings.json` or environment variables:
- `DatabaseConnection`: PostgreSQL connection string
- `Auth:Jwt:Issuer`, `Auth:Jwt:Audience`: JWT issuer and audience validation
- `Auth:Jwt:ClockSkew`: token validation clock skew (e.g., "00:00:00" for zero skew)
- `Auth:Jwt:TimeToLive`: token expiration time
- `Auth:Jwt:RsaKeyPath`: path to RSA private key PEM file for JWT signing

Swagger XML comments are loaded from `Cloudbb.Web.xml` and enabled in Development.

## Authentication
JWT is configured in `Startup.cs` (`JwtBearerDefaults`) with configurable clock skew. Services:
- `IJwtAlgorithmProvider` → `RsaSha256AlgorithmProvider` (RS256 algorithm)
- `IJwtRsaSigningKeyImportService` → `JwtRsaPemFileSigningKeyImportService` (PEM key loading)
- `IJwtSigningKeyProvider` → `JwtRsaSigningKeyProvider` (RSA key management)
- `IJwtService` → `JwtService` (token generation and validation)
- `IUserClaimIndex` → `UserClaimIndex` (user claim management)
- `ITimingRandomizationService` → `CsprngTimingRandomizationService` (security timing)

Tokens include claims for `NameIdentifier`, `Name`, `Email`, `Role`, `Jti`, `Iat`. TTL from `Auth:Jwt:TimeToLive`.

**Security**: Uses RSA RS256 asymmetric signing with PEM-format private keys for enhanced security over symmetric HMAC approaches.

## Data & Migrations
- `CloudbbDbContext` extends `IdentityDbContext<IdentityUser>` and enforces explicit entity/property mapping policies
- **Core Entities** in `Data/Model/`:
    - `CloudbbUser`: extends `CloudbbEntity`, bridges Identity system with forum features
    - `CloudbbPost`: forum posts with `CloudbbPostRevision` for edit history
    - `CloudbbComment`: threaded comments on posts
    - `CloudbbPostVote`/`CloudbbCommentVote`: community voting system (+1/-1/0)
- All entities extend `CloudbbEntity` (connection entities implement `ICloudbbConnectionEntity`)
- Migrations live in `Data/Migrations` and auto-apply at startup via `Database.MigrateAsync()`
- Seeded roles: `admin`, `user` via `Data/Seeds/IdentityRoleDataSeed.cs`
- **Custom migration helper**: `./add-migration.sh MigrationName` validates PascalCase and outputs to `Data/Migrations`

## API Versioning & Swagger
- Default API version is v1.0 with URL substitution (`api/v{version}/...`).
- Controllers should use `[ApiVersion("1.0")]` and routes like `api/v1/<resource>`.
- Development-only Swagger UI exposes per-version endpoints and Bearer auth.

## Build & Run
Use the following commands from the repo root:

```bash
# Build solution
dotnet build source/Cloudbb/Cloudbb.slnx

# Run the Web API (https://localhost:7156)
dotnet run --project source/Cloudbb/Cloudbb.Web

# Run the Blazor Client (https://localhost:7089)
dotnet run --project source/Cloudbb/Cloudbb.Client

# Run unit tests
dotnet test source/Cloudbb/Cloudbb.Web.Tests

# Run integration tests
dotnet test source/Cloudbb/Cloudbb.Web.Tests.Integration

# Run all tests
dotnet test source/Cloudbb/Cloudbb.slnx
```

### Development Setup
To run both the API and client together for local development:

1. **Configure the API** (see Configuration section):
   - Generate RSA key for JWT signing: `openssl genrsa -out rsa_key.pem 2048`
   - Set up PostgreSQL database
   - Update `appsettings.Development.json` with connection string and key path

2. **Run the API**: `dotnet run --project source/Cloudbb/Cloudbb.Web`
3. **Run the Client**: `dotnet run --project source/Cloudbb/Cloudbb.Client`

The client is pre-configured to connect to the API at `https://localhost:7156` and runs on `https://localhost:7089`. CORS is configured to allow client requests.

Database migrations:

```bash
# From source/Cloudbb/Cloudbb.Web/
./add-migration.sh MigrationName  # Validates PascalCase, outputs to Data/Migrations
dotnet ef database update
```

## Testing
Comprehensive test coverage using **MSTest v4**:

- **Unit Tests** (`Cloudbb.Web.Tests`): JWT services, authentication, algorithm providers
- **Integration Tests** (`Cloudbb.Web.Tests.Integration`): Full API testing with PostgreSQL
    - Transactional test isolation with automatic rollback
    - Complete controller coverage: Auth, Posts, Comments
    - Authentication scenarios, CRUD operations, voting, error handling
    - Static test data via `IntegrationTestDbLoader` for consistent testing

**Test Execution**: All tests use PostgreSQL test database with automatic transaction management.

## Adding Features
- **Controllers**: place under `Api/V1/Controllers/`, split into `.api.cs` (routes/docs) and `.cs` (logic), secure with `[Authorize]`
- **Models**: add to `Data/Model/`, extend `CloudbbEntity`, ensure explicit mapping policies in `CloudbbModelLoader`
- **Services**: add under `Services/` and register in `Startup.cs` with explicit lifetimes (`AddSingleton`/`AddScoped`)
- **Tests**: add unit tests to `Cloudbb.Web.Tests` and integration tests to `Cloudbb.Web.Tests.Integration`
- **API Models**: request/response models in `Api/V1/Models/` with data annotations and XML documentation

## Development Notes
- API versioning group format is `'v'VVV`.
- Swagger references are non-nullable and include validation filters.
- JWT issuer/audience/key and DB connection are read from configuration.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.