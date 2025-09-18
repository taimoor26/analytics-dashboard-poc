export interface SensorReading {
  Id: number;
  Timestamp: string;
  SensorId: string;
  Value: number;
  Unit: string;
  Location: string;
  IsAnomaly: boolean;
}

export interface SensorStats {
  window: string;
  sensorId?: string;
  totalReadings: number;
  totalAnomalies: number;
  sensors: {
    sensorId: string;
    count: number;
    anomalies: number;
    min: number;
    max: number;
    average: number;
    p95: number;
  }[];
  recentReadings: SensorReading[];
}

export interface Sensor {
  sensorId: string;
  location: string;
  unit: string;
  latestValue: number;
  latestTimestamp: string;
  isAnomaly: boolean;
}

export interface Alert {
  id: number;
  timestamp: string;
  sensorId: string;
  message: string;
  value: number;
  severity: string;
}
