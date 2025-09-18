import type { Alert } from '../types/sensor';
import './AlertsPanel.css';

interface AlertsPanelProps {
  alerts: Alert[];
  isLoading: boolean;
}

export const AlertsPanel: React.FC<AlertsPanelProps> = ({ alerts, isLoading }) => {
  if (isLoading) {
    return (
      <div className="alerts-panel">
        <h3>Alerts</h3>
        <p>Loading...</p>
      </div>
    );
  }

  const getSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'high': return '#e74c3c';
      case 'medium': return '#f39c12';
      case 'low': return '#f1c40f';
      default: return '#95a5a6';
    }
  };

  return (
    <div className="alerts-panel">
      <h3>Recent Alerts ({alerts.length})</h3>
      
      {alerts.length === 0 ? (
        <p className="no-alerts">No alerts in the last 24 hours</p>
      ) : (
        <div className="alerts-list">
          {alerts.map((alert) => (
            <div key={alert.id} className="alert-item">
              <div className="alert-header">
                <span className="alert-sensor">{alert.sensorId}</span>
                <span 
                  className="alert-severity"
                  style={{ backgroundColor: getSeverityColor(alert.severity) }}
                >
                  {alert.severity}
                </span>
              </div>
              <div className="alert-message">{alert.message}</div>
              <div className="alert-details">
                <span className="alert-value">Value: {alert.value.toFixed(2)}</span>
                <span className="alert-time">
                  {new Date(alert.timestamp).toLocaleString()}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}

    </div>
  );
};
