using Account.Api;
using Account.Data;
using Account.Observability;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI (see https://aka.ms/aspnet/openapi)
builder.Services.AddOpenApi();

// OpenTelemetry: traces, metrics, logs (OTLP + console in dev).
builder.Services.AddAccountTelemetry(builder.Configuration, builder.Environment);

// EF Core with PostgreSQL. Connection string resolved from configuration,
// which is sourced from AWS Secrets Manager / SSM in deployed environments.
var connectionString = builder.Configuration.GetConnectionString("AccountsDb")
    ?? "Host=localhost;Port=5432;Database=accounts;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AccountsDbContext>(options =>
    options.UseNpgsql(connectionString));

// Health checks: liveness at /health/live, readiness (incl. DB) at /health.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AccountsDbContext>(name: "database");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapAccountEndpoints();

// Readiness (verifies the database is reachable) and a lightweight liveness probe.
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();

// Exposed for integration testing via WebApplicationFactory.
public partial class Program;
