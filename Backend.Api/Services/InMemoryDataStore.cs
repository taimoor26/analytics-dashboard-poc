using System.Collections.Concurrent;
using Backend.Api.Models;

namespace Backend.Api.Services;

public class InMemoryDataStore
{
    private readonly ConcurrentQueue<SensorReading> _readings = new();
    private readonly object _lock = new();
    private readonly int _maxCapacity;
    private long _nextId = 1;

    public InMemoryDataStore(int maxCapacity = 100_000)
    {
        _maxCapacity = maxCapacity;
    }

    public void AddReading(SensorReading reading)
    {
        lock (_lock)
        {
            // Remove oldest if at capacity
            while (_readings.Count >= _maxCapacity)
            {
                _readings.TryDequeue(out _);
            }

            _readings.Enqueue(reading with { Id = _nextId++ });
        }
    }

    public IReadOnlyList<SensorReading> GetRecentReadings(int count = 1000)
    {
        lock (_lock)
        {
            return _readings.TakeLast(count).ToList();
        }
    }

    public IReadOnlyList<SensorReading> GetReadingsBySensor(string sensorId, int count = 1000)
    {
        lock (_lock)
        {
            return _readings
                .Where(r => r.SensorId == sensorId)
                .TakeLast(count)
                .ToList();
        }
    }

    public IReadOnlyList<SensorReading> GetReadingsInTimeRange(DateTime start, DateTime end)
    {
        lock (_lock)
        {
            return _readings
                .Where(r => r.Timestamp >= start && r.Timestamp <= end)
                .OrderBy(r => r.Timestamp)
                .ToList();
        }
    }

    public SensorReading? GetLatestReading(string sensorId)
    {
        lock (_lock)
        {
            return _readings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault();
        }
    }

    public int Count => _readings.Count;

    public void Clear()
    {
        lock (_lock)
        {
            while (_readings.TryDequeue(out _)) { }
        }
    }
}
