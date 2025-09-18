using System.Text;
using System.Text.Json;
using Backend.Api.Models;
using Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Endpoints;

public static class StreamEndpoints
{
    public static void MapStreamEndpoints(this WebApplication app)
    {
        app.MapGet("/stream", StreamSensorData)
           .WithName("StreamSensorData")
           .WithOpenApi();

        app.MapGet("/stream/{sensorId}", StreamSensorDataById)
           .WithName("StreamSensorDataById")
           .WithOpenApi();
    }

    private static async Task StreamSensorData(
        InMemoryDataStore dataStore,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        context.Response.Headers["Content-Type"] = "text/event-stream";
        context.Response.Headers["Cache-Control"] = "no-cache";
        context.Response.Headers["Connection"] = "keep-alive";
        context.Response.Headers["Access-Control-Allow-Origin"] = "*";

        var lastSentId = 0L;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var recentReadings = dataStore.GetRecentReadings(100);
                var newReadings = recentReadings.Where(r => r.Id > lastSentId).ToList();

                if (newReadings.Any())
                {
                    foreach (var reading in newReadings)
                    {
                        var json = JsonSerializer.Serialize(reading);
                        var data = $"data: {json}\n\n";
                        var bytes = Encoding.UTF8.GetBytes(data);
                        
                        await context.Response.Body.WriteAsync(bytes, cancellationToken);
                        await context.Response.Body.FlushAsync(cancellationToken);
                        
                        lastSentId = Math.Max(lastSentId, reading.Id);
                    }
                }

                await Task.Delay(100, cancellationToken); // Send updates every 100ms
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected
        }
        catch (Exception ex)
        {
            var errorData = $"data: {JsonSerializer.Serialize(new { error = ex.Message })}\n\n";
            var errorBytes = Encoding.UTF8.GetBytes(errorData);
            await context.Response.Body.WriteAsync(errorBytes, cancellationToken);
        }
    }

    private static async Task StreamSensorDataById(
        string sensorId,
        InMemoryDataStore dataStore,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        context.Response.Headers["Content-Type"] = "text/event-stream";
        context.Response.Headers["Cache-Control"] = "no-cache";
        context.Response.Headers["Connection"] = "keep-alive";
        context.Response.Headers["Access-Control-Allow-Origin"] = "*";

        var lastSentId = 0L;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var recentReadings = dataStore.GetReadingsBySensor(sensorId, 100);
                var newReadings = recentReadings.Where(r => r.Id > lastSentId).ToList();

                if (newReadings.Any())
                {
                    foreach (var reading in newReadings)
                    {
                        var json = JsonSerializer.Serialize(reading);
                        var data = $"data: {json}\n\n";
                        var bytes = Encoding.UTF8.GetBytes(data);
                        
                        await context.Response.Body.WriteAsync(bytes, cancellationToken);
                        await context.Response.Body.FlushAsync(cancellationToken);
                        
                        lastSentId = Math.Max(lastSentId, reading.Id);
                    }
                }

                await Task.Delay(100, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected
        }
        catch (Exception ex)
        {
            var errorData = $"data: {JsonSerializer.Serialize(new { error = ex.Message })}\n\n";
            var errorBytes = Encoding.UTF8.GetBytes(errorData);
            await context.Response.Body.WriteAsync(errorBytes, cancellationToken);
        }
    }
}
