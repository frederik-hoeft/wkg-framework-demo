# Cloudbb .NET Service

Cloudbb is an ASP.NET Core 10.0 Web API using PostgreSQL, ASP.NET Core Identity, and JWT auth. The project includes API versioning, grouped Swagger docs, EF Core migrations with auto-apply on startup, and strict code conventions for clarity and maintainability.

## Overview
- **Core app**: `source/Cloudbb/Cloudbb.Web` (`net10.0`, nullable enabled).
- **Auth**: ASP.NET Core Identity + JWT (HMAC-SHA256 symmetric signing), global authorization.
- **Data**: PostgreSQL via EF Core with explicit entity naming/mapping and model discovery.
- **API Versioning**: `Asp.Versioning` with grouped Swagger in Development.
- **Transactions**: `ReadCommitted` via `Wkg.AspNetCore.Transactions`.

## Project Structure
- `source/Cloudbb/Cloudbb.Web`
    - `Api/V1/Controllers/`: versioned controllers split into `.api.cs` (routing/docs) and `.cs` (logic)
    - `Configuration/Swagger/`: Swagger configuration and filters
    - `Configuration/Versions.cs`: API version constants
    - `Data/`: DbContext, model loader, entities, seeds, migrations
    - `Services/Auth/`: JWT and auth-related services
    - `Extensions/`: helpers (e.g., path utilities)

## Conventions
See `code-style.md`. Highlights:
- No `var`; use explicit types. Async methods end with `Async`.
- File-scoped namespaces; Allman bracing; explicit visibility; sealed internal types.
- Target-typed `new` only when the left-hand type is explicit.

## Configuration
Set these in `appsettings.json` or environment variables:
- `DatabaseConnection`: PostgreSQL connection string
- `Auth:Jwt:Issuer`, `Auth:Jwt:Audience`, `Auth:Jwt:Key`, `Auth:Jwt:TimeToLive`

Swagger XML comments are loaded from `Cloudbb.Web.xml` and enabled in Development.

## Authentication
JWT is configured in `Program.cs` (`JwtBearerDefaults`) with zero clock skew. Services:
- `IJwtAlgorithmProvider` → `JwtHmacSha256AlgorithmProvider`
- `IJwtSigningKeyProvider` → `JwtSymmetricSigningKeyProvider`
- `IJwtService` → `JwtService`

Tokens include claims for `NameIdentifier`, `Name`, `Email`, `Role`, `Jti`, `Iat`. TTL from `Auth:Jwt:TimeToLive`.

## Data & Migrations
- `ApplicationDbContext` extends `IdentityDbContext<IdentityUser>` and enforces explicit entity/property mapping policies.
- Entities live in `Data/Model/` and typically extend `CloudbbEntity` (connection entities can implement `ICloudbbConnectionEntity`).
- Migrations live in `Data/Migrations` and auto-apply at startup via `Database.MigrateAsync()`.
- Seeded roles: `admin`, `user` via `Data/Seeds/IdentityRoleDataSeed.cs`.

## API Versioning & Swagger
- Default API version is v1.0 with URL substitution (`api/v{version}/...`).
- Controllers should use `[ApiVersion("1.0")]` and routes like `api/v1/<resource>`.
- Development-only Swagger UI exposes per-version endpoints and Bearer auth.

## Build & Run
Use the following commands from the repo root (Windows bash):

```bash
# Build solution
dotnet build source/Cloudbb/Cloudbb.slnx

# Run the Web API
dotnet run --project source/Cloudbb/Cloudbb.Web
```

Database migrations:

```bash
# From source/Cloudbb/Cloudbb.Web/
dotnet ef migrations add MigrationName
dotnet ef database update

# Preferred helper (validates PascalCase name, writes to Data/Migrations)
bash source/Cloudbb/Cloudbb.Web/add-migration.sh AddMyEntity
```

## Adding Features
- **Controllers**: place under `Api/V1/Controllers/`, split into `.api.cs` and `.cs`, secure with `[Authorize]` where required.
- **Models**: add to `Data/Model/` and ensure they meet mapping/inheritance policies; include in `ApplicationModelLoader` discovery as needed.
- **Services**: add under `Services/` and register in `Program.cs` with explicit lifetimes (`AddSingleton`/`AddScoped`).

## Development Notes
- API versioning group format is `'v'VVV`.
- Swagger references are non-nullable and include validation filters.
- JWT issuer/audience/key and DB connection are read from configuration.

## License
This repository does not declare a license. If you intend to distribute or open-source, please add a license file.