using Microsoft.EntityFrameworkCore;
using Backend.Api.Data;
using Backend.Api.Models;

namespace Backend.Api.Services;

public class AggregationService : BackgroundService
{
    private readonly InMemoryDataStore _dataStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AggregationService> _logger;
    private readonly Timer _cleanupTimer;

    public AggregationService(
        InMemoryDataStore dataStore,
        IServiceProvider serviceProvider,
        ILogger<AggregationService> logger)
    {
        _dataStore = dataStore;
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Clean up old data every hour
        _cleanupTimer = new Timer(CleanupOldData, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Aggregation service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AggregateRecentData();
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Aggregate every 10 seconds
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during aggregation");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        _cleanupTimer?.Dispose();
        _logger.LogInformation("Aggregation service stopped");
    }

    private async Task AggregateRecentData()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

        // Get readings from the last 10 seconds
        var cutoff = DateTime.UtcNow.AddSeconds(-10);
        var recentReadings = _dataStore.GetReadingsInTimeRange(cutoff, DateTime.UtcNow);

        if (!recentReadings.Any())
            return;

        // Group by sensor and 1-second buckets
        var aggregates = recentReadings
            .GroupBy(r => new { r.SensorId, Bucket = r.Timestamp.TruncateToSecond() })
            .Select(g => new SensorAggregate(
                BucketStart: g.Key.Bucket,
                SensorId: g.Key.SensorId,
                Count: g.Count(),
                Min: g.Min(x => x.Value),
                Max: g.Max(x => x.Value),
                Average: g.Average(x => x.Value),
                P95: CalculatePercentile(g.Select(x => x.Value), 0.95),
                AnomalyCount: g.Count(x => x.IsAnomaly)
            ))
            .ToList();

        if (aggregates.Any())
        {
            // Upsert aggregates
            foreach (var aggregate in aggregates)
            {
                var existing = await context.SensorAggregates
                    .FirstOrDefaultAsync(a => a.BucketStart == aggregate.BucketStart && a.SensorId == aggregate.SensorId);

                if (existing != null)
                {
                    // Update existing
                    existing = existing with
                    {
                        Count = aggregate.Count,
                        Min = aggregate.Min,
                        Max = aggregate.Max,
                        Average = aggregate.Average,
                        P95 = aggregate.P95,
                        AnomalyCount = aggregate.AnomalyCount
                    };
                }
                else
                {
                    // Add new
                    context.SensorAggregates.Add(aggregate);
                }
            }

            await context.SaveChangesAsync();
            _logger.LogDebug("Aggregated {Count} sensor buckets", aggregates.Count);
        }
    }

    private void CleanupOldData(object? state)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            // Delete data older than 24 hours
            var cutoff = DateTime.UtcNow.AddHours(-24);
            
            var oldAggregates = context.SensorAggregates
                .Where(a => a.BucketStart < cutoff)
                .ToList();

            var oldAlerts = context.Alerts
                .Where(a => a.Timestamp < cutoff)
                .ToList();

            if (oldAggregates.Any())
            {
                context.SensorAggregates.RemoveRange(oldAggregates);
                _logger.LogInformation("Removed {Count} old aggregates", oldAggregates.Count);
            }

            if (oldAlerts.Any())
            {
                context.Alerts.RemoveRange(oldAlerts);
                _logger.LogInformation("Removed {Count} old alerts", oldAlerts.Count);
            }

            context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
        }
    }

    private static double CalculatePercentile(IEnumerable<double> values, double percentile)
    {
        var sorted = values.OrderBy(x => x).ToArray();
        if (!sorted.Any()) return 0;

        var index = (int)Math.Ceiling(percentile * sorted.Length) - 1;
        index = Math.Max(0, Math.Min(index, sorted.Length - 1));
        return sorted[index];
    }
}

public static class DateTimeExtensions
{
    public static DateTime TruncateToSecond(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 
                           dateTime.Hour, dateTime.Minute, dateTime.Second, 
                           DateTimeKind.Utc);
    }
}
