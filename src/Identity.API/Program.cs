using Core.Defaults;
using Identity.API.Apis;
using Identity.API.Extensions;
using Identity.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureIdentitySerilog();
builder.ConfigureOpenTelemetry();

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddOpenApiDocumentation()
    .AddCustomHealthChecks(builder.Configuration);

builder.Services.AddAuthorizationPolicies();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
}

app.UseOpenApiDocumentation();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUsersEndpoints();

app.Run();