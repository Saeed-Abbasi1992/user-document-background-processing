using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserDocumentProcessor.Infrastructure.Persistence;

namespace UserDocumentProcessor.API.HealthChecks;

public static class HealthChecksSetup
{
    public static IServiceCollection AddProjectHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API is alive"))
            .AddDbContextCheck<AppDbContext>("Database", HealthStatus.Unhealthy, tags: new[] { "ready" })
            .AddCheck<HangfireHealthCheck>("Hangfire", tags: new[] { "ready" });

        return services;
    }
}