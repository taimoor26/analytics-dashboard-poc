namespace Backend.Api.Models;

public record SensorReading(
    long Id,
    DateTime Timestamp,
    string SensorId,
    double Value,
    string Unit,
    string Location,
    bool IsAnomaly = false
);

public record SensorAggregate(
    DateTime BucketStart,
    string SensorId,
    int Count,
    double Min,
    double Max,
    double Average,
    double P95,
    int AnomalyCount
);

public record Alert(
    long Id,
    DateTime Timestamp,
    string SensorId,
    string Message,
    double Value,
    string Severity
);
