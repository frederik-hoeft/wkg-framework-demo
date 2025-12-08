using Cloudbb.Web.Data;
using Cloudbb.Web.Services.Auth;
using Cloudbb.Web.Services.Auth.Default;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using Wkg.AspNetCore.Transactions.Configuration;
using Wkg.EntityFrameworkCore.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
// use source-generated model discovery for better startup performance and compile-time model validation
builder.Services.AddSingleton<IModelLoader, ApplicationModelLoader>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnection")));

// Add Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
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
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Auth:Jwt:Issuer"],
    ValidAudience = builder.Configuration["Auth:Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Auth:Jwt:Key"]!)),
    ClockSkew = TimeSpan.Zero
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransactionManagement<ApplicationDbContext>(transactionOptions => transactionOptions
    .UseIsolationLevel(IsolationLevel.ReadCommitted));

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                            Register application services                                                 //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// auth services
//builder.Services.AddSingleton<IJwtECDsaSigningKeyImportService, JwtECDsaPemFileSigningKeyImportService>();
builder.Services.AddSingleton<IJwtAlgorithmProvider, JwtHmacSha256AlgorithmProvider>();
builder.Services.AddSingleton<IJwtSigningKeyProvider, JwtSymmetricSigningKeyProvider>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserClaimIndex, UserClaimIndex>();
builder.Services.AddSingleton<ITimingRandomizationService, CsprngTimingRandomizationService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

await app.RunAsync();
