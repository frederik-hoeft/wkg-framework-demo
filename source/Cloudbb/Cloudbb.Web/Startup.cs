using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Cloudbb.Web.Configuration.Swagger;
using Cloudbb.Web.Data;
using Cloudbb.Web.Services.Auth;
using Cloudbb.Web.Services.Auth.Default;
using Cloudbb.Web.Services.Auth.Policies;
using Cloudbb.Web.Services.Versioning;
using Cloudbb.Web.Services.Versioning.Default;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wkg.AspNetCore.Configuration;
using Wkg.AspNetCore.Transactions.Configuration;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web;

internal sealed class Startup : IAsyncStartupScript
{
    public static ValueTask ConfigureServicesAsync(IServiceCollection services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        // Add Entity Framework
        // use source-generated model discovery for better startup performance and compile-time model validation
        services.AddSingleton<IModelLoader, CloudbbModelLoader>();
        services.AddDbContext<CloudbbDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DatabaseConnection")));

        // Add Identity services
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequiredLength = 16;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<CloudbbDbContext>()
        .AddDefaultTokenProviders();

        // Add JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            JwtRsaPemFileSigningKeyImportService keyImportService = new(configuration);
            JwtRsaSigningKeyProvider keyLoader = new(keyImportService);
            Task<SecurityKey> keyTask = keyLoader.GetKeyAsync().AsTask();
            keyTask.Wait();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = configuration["Auth:Jwt:Issuer"],
                ValidAudience = configuration["Auth:Jwt:Audience"],
                IssuerSigningKey = keyTask.Result,
                ClockSkew = TimeSpan.Parse(configuration["Auth:Jwt:ClockSkew"]!),
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.User.Name, policy => policy.RequireRole(AuthPolicies.User.Roles))
            .AddPolicy(AuthPolicies.Admin.Name, policy => policy.RequireRole(AuthPolicies.Admin.Roles));

        services.AddHttpContextAccessor();

        services.AddTransactionManagement<CloudbbDbContext>(transactionOptions => transactionOptions
            .UseIsolationLevel(IsolationLevel.ReadCommitted));

        services.AddHealthChecks();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                            Register application services                                                 //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // auth services
        services.AddSingleton<IJwtAlgorithmProvider, RsaSha256AlgorithmProvider>();
        services.AddSingleton<IJwtRsaSigningKeyImportService, JwtRsaPemFileSigningKeyImportService>();
        services.AddSingleton<IJwtSigningKeyProvider, JwtRsaSigningKeyProvider>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserClaimIndex, UserClaimIndex>();
        services.AddSingleton<ITimingRandomizationService, CsprngTimingRandomizationService>();
        services.AddSingleton<IVersionProvider, CloudbbWebVersionProvider>();

        services.AddControllers().AddJsonOptions(options =>
        {
            JsonNamingPolicy namingPolicy = JsonNamingPolicy.CamelCase;

            JsonStringEnumConverter enumConverter = new(namingPolicy);
            options.JsonSerializerOptions.Converters.Add(enumConverter);
            options.JsonSerializerOptions.PropertyNamingPolicy = namingPolicy;
            options.JsonSerializerOptions.WriteIndented = true;
        });
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        return ValueTask.CompletedTask;
    }

    public static async ValueTask ConfigureAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            IApiVersionDescriptionProvider versionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            app.UseSwagger();
            app.UseSwaggerUI(swagger =>
            {
                swagger.EnableDeepLinking();
                foreach (ApiVersionDescription description in versionDescriptionProvider.ApiVersionDescriptions)
                {
                    swagger.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.UseHttpMetrics(options => options.ConfigureMeasurements(measurementOptions =>
            // Only measure exemplar if the HTTP response status code is not "OK".
            measurementOptions.ExemplarPredicate = context => context.Response.StatusCode is not StatusCodes.Status200OK));

        app.MapMetrics("/metrics");

        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        await using CloudbbDbContext context = scope.ServiceProvider.GetRequiredService<CloudbbDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
