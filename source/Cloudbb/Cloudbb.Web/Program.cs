using Cloudbb.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// use named startup class to reference in integration tests
WebApplication app = await builder.BuildUsingAsync<Startup>();
await app.RunAsync();
