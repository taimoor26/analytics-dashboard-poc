import type { SensorStats, Sensor } from '../types/sensor';
import './StatsPanel.css';

interface StatsPanelProps {
  stats: SensorStats | null;
  sensors: Sensor[];
  isLoading: boolean;
}

export const StatsPanel: React.FC<StatsPanelProps> = ({ stats, sensors, isLoading }) => {
  if (isLoading) {
    return (
      <div className="stats-panel">
        <h3>Statistics</h3>
        <p>Loading...</p>
      </div>
    );
  }

  if (!stats) {
    return (
      <div className="stats-panel">
        <h3>Statistics</h3>
        <p>No data available</p>
      </div>
    );
  }

  return (
    <div className="stats-panel">
      <h3>Statistics ({stats.window})</h3>
      
      <div className="stats-grid">
        <div className="stat-card">
          <h4>Total Readings</h4>
          <p className="stat-value">{stats.totalReadings?.toLocaleString() || '0'}</p>
        </div>
        
        <div className="stat-card">
          <h4>Anomalies</h4>
          <p className="stat-value anomaly">{stats.totalAnomalies?.toLocaleString() || '0'}</p>
        </div>
        
        <div className="stat-card">
          <h4>Active Sensors</h4>
          <p className="stat-value">{sensors.length}</p>
        </div>
      </div>

      <div className="sensor-stats">
        <h4>Sensor Details</h4>
        {stats.sensors?.map((sensor) => (
          <div key={sensor.sensorId} className="sensor-card">
            <div className="sensor-header">
              <h5>{sensor.sensorId}</h5>
              <span className={`anomaly-badge ${(sensor.anomalies || 0) > 0 ? 'has-anomalies' : ''}`}>
                {sensor.anomalies || 0} anomalies
              </span>
            </div>
            <div className="sensor-metrics">
              <div className="metric">
                <span>Count:</span>
                <span>{sensor.count || 0}</span>
              </div>
              <div className="metric">
                <span>Min:</span>
                <span>{(sensor.min || 0).toFixed(2)}</span>
              </div>
              <div className="metric">
                <span>Max:</span>
                <span>{(sensor.max || 0).toFixed(2)}</span>
              </div>
              <div className="metric">
                <span>Avg:</span>
                <span>{(sensor.average || 0).toFixed(2)}</span>
              </div>
              <div className="metric">
                <span>P95:</span>
                <span>{(sensor.p95 || 0).toFixed(2)}</span>
              </div>
            </div>
          </div>
        )) || []}
      </div>

    </div>
  );
};
