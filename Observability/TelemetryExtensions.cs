using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Account.Observability;

/// <summary>
/// Centralized OpenTelemetry wiring for traces, metrics, and logs.
///
/// Export strategy (free / cheapest-first):
///   - OTLP exporter targets an OpenTelemetry Collector. Point it at any backend
///     (self-hosted Grafana Tempo/Prometheus/Loki or Jaeger = free; or ADOT ->
///     CloudWatch/X-Ray on AWS = pay-per-use, no fixed cost).
///   - Console exporter is enabled in Development for zero-cost local visibility.
///
/// Endpoint is configured via the standard OTEL_EXPORTER_OTLP_ENDPOINT env var,
/// so no code change is needed to switch backends. If unset, OTLP export is
/// skipped (only console in dev), avoiding noisy connection errors locally.
/// </summary>
public static class TelemetryExtensions
{
    public const string ServiceName = "account-service";

    // Custom ActivitySource/Meter for application-level instrumentation.
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);

    public static IServiceCollection AddAccountTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        var hasOtlp = !string.IsNullOrWhiteSpace(otlpEndpoint);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: ServiceName,
                serviceVersion: typeof(TelemetryExtensions).Assembly.GetName().Version?.ToString() ?? "unknown")
            .AddAttributes(new KeyValuePair<string, object>[]
            {
                new("deployment.environment", environment.EnvironmentName)
            });

        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(ServiceName)
                .AddAttributes(new KeyValuePair<string, object>[]
                {
                    new("deployment.environment", environment.EnvironmentName)
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ServiceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                // Explicit static call to disambiguate from the EF Core
                // IServiceCollection.AddNpgsql extension of the same name.
                Npgsql.TracerProviderBuilderExtensions.AddNpgsql(tracing);

                if (environment.IsDevelopment())
                {
                    tracing.AddConsoleExporter();
                }

                if (hasOtlp)
                {
                    tracing.AddOtlpExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(ServiceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (environment.IsDevelopment())
                {
                    metrics.AddConsoleExporter();
                }

                if (hasOtlp)
                {
                    metrics.AddOtlpExporter();
                }
            });

        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(otel =>
            {
                otel.SetResourceBuilder(resourceBuilder);
                otel.IncludeFormattedMessage = true;
                otel.IncludeScopes = true;

                if (environment.IsDevelopment())
                {
                    otel.AddConsoleExporter();
                }

                if (hasOtlp)
                {
                    otel.AddOtlpExporter();
                }
            });
        });

        return services;
    }
}
