using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserDocumentProcessor.API.HealthChecks;

public class HangfireHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var stats = monitoringApi.GetStatistics();

            if (stats == null)
                return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire monitoring API unavailable"));

            return Task.FromResult(HealthCheckResult.Healthy("Hangfire is healthy"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire check failed", ex));
        }
    }
}