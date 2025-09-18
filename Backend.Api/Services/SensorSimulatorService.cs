using System.Collections.Concurrent;
using Backend.Api.Models;

namespace Backend.Api.Services;

public class SensorSimulatorService : BackgroundService
{
    private readonly InMemoryDataStore _dataStore;
    private readonly ILogger<SensorSimulatorService> _logger;
    private readonly Random _random = new();
    private readonly string[] _sensorIds = { "temp-001", "temp-002", "pressure-001", "humidity-001", "vibration-001" };
    private readonly string[] _locations = { "Building A", "Building B", "Factory Floor", "Storage Room", "Office" };
    private readonly string[] _units = { "°C", "Pa", "%", "Hz", "V" };

    // Anomaly detection state
    private readonly ConcurrentDictionary<string, double> _sensorBaselines = new();
    private readonly ConcurrentDictionary<string, double> _sensorStdDevs = new();

    public SensorSimulatorService(InMemoryDataStore dataStore, ILogger<SensorSimulatorService> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sensor simulator started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Generate 1000 readings per second (1000ms / 1000 = 1ms per reading)
                // But we'll batch them for efficiency
                var batchSize = 10;
                var delayMs = 10; // 10ms delay = 100 batches per second = 1000 readings per second

                for (int i = 0; i < batchSize; i++)
                {
                    var reading = GenerateSensorReading();
                    _dataStore.AddReading(reading);
                }

                await Task.Delay(delayMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sensor readings");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("Sensor simulator stopped");
    }

    private SensorReading GenerateSensorReading()
    {
        var sensorId = _sensorIds[_random.Next(_sensorIds.Length)];
        var location = _locations[_random.Next(_locations.Length)];
        var unit = _units[_random.Next(_units.Length)];
        var timestamp = DateTime.UtcNow;

        // Generate realistic sensor values based on sensor type
        var (value, isAnomaly) = GenerateSensorValue(sensorId, unit);

        return new SensorReading(
            Id: 0, // Will be set by data store
            Timestamp: timestamp,
            SensorId: sensorId,
            Value: value,
            Unit: unit,
            Location: location,
            IsAnomaly: isAnomaly
        );
    }

    private (double value, bool isAnomaly) GenerateSensorValue(string sensorId, string unit)
    {
        double baseline, stdDev, value;
        bool isAnomaly = false;

        // Initialize baseline and std dev for new sensors
        if (!_sensorBaselines.ContainsKey(sensorId))
        {
            baseline = GetBaselineForSensorType(sensorId, unit);
            stdDev = baseline * 0.1; // 10% standard deviation
            _sensorBaselines[sensorId] = baseline;
            _sensorStdDevs[sensorId] = stdDev;
        }
        else
        {
            baseline = _sensorBaselines[sensorId];
            stdDev = _sensorStdDevs[sensorId];
        }

        // Generate value with normal distribution
        value = GenerateNormalDistribution(baseline, stdDev);

        // Occasionally introduce anomalies (5% chance)
        if (_random.NextDouble() < 0.05)
        {
            value = GenerateAnomalyValue(baseline, stdDev);
            isAnomaly = true;
        }

        // Update baseline slowly (moving average)
        _sensorBaselines[sensorId] = baseline * 0.99 + value * 0.01;
        _sensorStdDevs[sensorId] = stdDev * 0.99 + Math.Abs(value - baseline) * 0.01;

        return (value, isAnomaly);
    }

    private double GetBaselineForSensorType(string sensorId, string unit)
    {
        return unit switch
        {
            "°C" => 22.0, // Temperature
            "Pa" => 101325.0, // Pressure
            "%" => 45.0, // Humidity
            "Hz" => 50.0, // Vibration
            "V" => 3.3, // Voltage
            _ => 0.0
        };
    }

    private double GenerateNormalDistribution(double mean, double stdDev)
    {
        // Box-Muller transform
        var u1 = _random.NextDouble();
        var u2 = _random.NextDouble();
        var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * z0;
    }

    private double GenerateAnomalyValue(double baseline, double stdDev)
    {
        // Generate extreme values (3+ standard deviations from mean)
        var multiplier = _random.NextDouble() < 0.5 ? -1 : 1;
        var anomalyFactor = 3.0 + _random.NextDouble() * 2.0; // 3-5 std devs
        return baseline + multiplier * stdDev * anomalyFactor;
    }
}
