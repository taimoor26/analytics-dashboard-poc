using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Data;
using Backend.Api.Models;
using Backend.Api.Services;

namespace Backend.Api.Endpoints;

public static class StatsEndpoints
{
    public static void MapStatsEndpoints(this WebApplication app)
    {
        app.MapGet("/stats", GetStats)
           .WithName("GetStats")
           .WithOpenApi();

        app.MapGet("/stats/{sensorId}", GetStatsBySensor)
           .WithName("GetStatsBySensor")
           .WithOpenApi();

        app.MapGet("/alerts", GetAlerts)
           .WithName("GetAlerts")
           .WithOpenApi();

        app.MapGet("/sensors", GetSensors)
           .WithName("GetSensors")
           .WithOpenApi();
    }

    private static async Task<IResult> GetStats(
        AnalyticsDbContext context,
        InMemoryDataStore dataStore,
        [FromQuery] string? window = "60s",
        [FromQuery] string? sensorId = null)
    {
        var windowSeconds = ParseWindow(window);
        var cutoff = DateTime.UtcNow.AddSeconds(-windowSeconds);

        var query = context.SensorAggregates
            .Where(a => a.BucketStart >= cutoff);

        if (!string.IsNullOrEmpty(sensorId))
        {
            query = query.Where(a => a.SensorId == sensorId);
        }

        var aggregates = await query.ToListAsync();

        var stats = new
        {
            Window = window,
            SensorId = sensorId,
            TotalReadings = aggregates.Sum(a => a.Count),
            TotalAnomalies = aggregates.Sum(a => a.AnomalyCount),
            Sensors = aggregates.GroupBy(a => a.SensorId).Select(g => new
            {
                SensorId = g.Key,
                Count = g.Sum(a => a.Count),
                Anomalies = g.Sum(a => a.AnomalyCount),
                Min = g.Min(a => a.Min),
                Max = g.Max(a => a.Max),
                Average = g.Average(a => a.Average),
                P95 = g.Average(a => a.P95)
            }).ToList(),
            RecentReadings = dataStore.GetRecentReadings(100)
        };

        return Results.Ok(stats);
    }

    private static async Task<IResult> GetStatsBySensor(
        string sensorId,
        AnalyticsDbContext context,
        InMemoryDataStore dataStore,
        [FromQuery] string? window = "60s")
    {
        var windowSeconds = ParseWindow(window);
        var cutoff = DateTime.UtcNow.AddSeconds(-windowSeconds);

        var aggregates = await context.SensorAggregates
            .Where(a => a.SensorId == sensorId && a.BucketStart >= cutoff)
            .ToListAsync();

        var latestReading = dataStore.GetLatestReading(sensorId);

        var stats = new
        {
            SensorId = sensorId,
            Window = window,
            TotalReadings = aggregates.Sum(a => a.Count),
            TotalAnomalies = aggregates.Sum(a => a.AnomalyCount),
            Min = aggregates.Any() ? aggregates.Min(a => a.Min) : 0,
            Max = aggregates.Any() ? aggregates.Max(a => a.Max) : 0,
            Average = aggregates.Any() ? aggregates.Average(a => a.Average) : 0,
            P95 = aggregates.Any() ? aggregates.Average(a => a.P95) : 0,
            LatestReading = latestReading
        };

        return Results.Ok(stats);
    }

    private static async Task<IResult> GetAlerts(
        AnalyticsDbContext context,
        [FromQuery] int limit = 100)
    {
        var alerts = await context.Alerts
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();

        return Results.Ok(alerts);
    }

    private static IResult GetSensors(InMemoryDataStore dataStore)
    {
        var recentReadings = dataStore.GetRecentReadings(1000);
        var sensors = recentReadings
            .GroupBy(r => r.SensorId)
            .Select(g => new
            {
                SensorId = g.Key,
                Location = g.First().Location,
                Unit = g.First().Unit,
                LatestValue = g.OrderByDescending(r => r.Timestamp).First().Value,
                LatestTimestamp = g.OrderByDescending(r => r.Timestamp).First().Timestamp,
                IsAnomaly = g.OrderByDescending(r => r.Timestamp).First().IsAnomaly
            })
            .OrderBy(s => s.SensorId)
            .ToList();

        return Results.Ok(sensors);
    }

    private static int ParseWindow(string? window)
    {
        if (string.IsNullOrEmpty(window))
            return 60;

        var unit = window[^1];
        var value = int.Parse(window[..^1]);

        return unit switch
        {
            's' => value,
            'm' => value * 60,
            'h' => value * 3600,
            'd' => value * 86400,
            _ => 60
        };
    }
}
