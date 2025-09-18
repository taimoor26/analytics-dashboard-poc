import { useEffect, useState, useCallback } from 'react';
import type { SensorReading, SensorStats, Sensor, Alert } from './types/sensor';
import { SensorApi } from './api/sensorApi';
import { LiveChart } from './components/LiveChart';
import { StatsPanel } from './components/StatsPanel';
import { AlertsPanel } from './components/AlertsPanel';
import './App.css';

function App() {
  const [readings, setReadings] = useState<SensorReading[]>([]);
  const [stats, setStats] = useState<SensorStats | null>(null);
  const [sensors, setSensors] = useState<Sensor[]>([]);
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const [selectedSensor, setSelectedSensor] = useState<string>('');
  const [isLoading, setIsLoading] = useState(true);

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      try {
        console.log('🔄 Loading initial data...');
        const [statsData, sensorsData, alertsData] = await Promise.all([
          SensorApi.getStats('60s'),
          SensorApi.getSensors(),
          SensorApi.getAlerts(50)
        ]);
        
        console.log('📊 Stats data loaded:', statsData);
        console.log('📊 Sensors data loaded:', sensorsData);
        console.log('📊 Alerts data loaded:', alertsData);
        
        setStats(statsData);
        setSensors(sensorsData);
        setAlerts(alertsData);
        setIsLoading(false);
      } catch (error) {
        console.error('❌ Failed to load initial data:', error);
        setIsLoading(false);
      }
    };

    loadInitialData();
  }, []);

  // Ensure page starts at top
  useEffect(() => {
    window.scrollTo(0, 0);
    document.documentElement.scrollTop = 0;
    document.body.scrollTop = 0;
  }, []);

  // Set up real-time streaming
  useEffect(() => {
    console.log('Setting up EventSource for sensor:', selectedSensor || 'all');
    const eventSource = SensorApi.createEventSource(selectedSensor || undefined);
    
    eventSource.onopen = () => {
      setIsConnected(true);
      console.log('✅ Connected to sensor stream');
    };

    eventSource.onmessage = (event) => {
      try {
        const reading: SensorReading = JSON.parse(event.data);
        // Only log every 100th reading to avoid console spam
        if (reading.Id % 100 === 0) {
          console.log('📊 Received reading #', reading.Id, ':', reading.SensorId, reading.Value);
        }
        setReadings(prev => {
          const newReadings = [...prev, reading];
          // Keep only last 1000 readings to prevent memory issues
          return newReadings.slice(-1000);
        });
      } catch (error) {
        console.error('❌ Failed to parse sensor reading:', error, event.data);
      }
    };

    eventSource.onerror = (error) => {
      console.error('❌ EventSource failed:', error);
      setIsConnected(false);
    };

    return () => {
      console.log('🔌 Closing EventSource');
      eventSource.close();
      setIsConnected(false);
    };
  }, [selectedSensor]);

  // Refresh stats periodically
  useEffect(() => {
    const interval = setInterval(async () => {
      try {
        console.log('🔄 Refreshing stats...');
        const statsData = await SensorApi.getStats('60s', selectedSensor || undefined);
        console.log('📊 Stats refreshed:', statsData);
        setStats(statsData);
      } catch (error) {
        console.error('❌ Failed to refresh stats:', error);
      }
    }, 5000); // Refresh every 5 seconds

    return () => clearInterval(interval);
  }, [selectedSensor]);

  const handleSensorChange = useCallback((sensorId: string) => {
    setSelectedSensor(sensorId);
    setReadings([]); // Clear readings when switching sensors
  }, []);

  return (
    <div className="app">
      <header className="app-header">
        <h1>Real-time Analytics Dashboard</h1>
        <div className="status-indicators">
          <div className={`status ${isConnected ? 'connected' : 'disconnected'}`}>
            {isConnected ? '🟢 Connected' : '🔴 Disconnected'}
          </div>
          <div className="readings-count">
            Readings: {readings.length}
          </div>
          <div className="debug-info">
            Sensors: {sensors.length} | Loading: {isLoading ? 'Yes' : 'No'}
          </div>
        </div>
      </header>

      <div className="controls">
        <div className="sensor-selector">
          <label htmlFor="sensor-select">Filter by Sensor:</label>
          <select 
            id="sensor-select"
            value={selectedSensor} 
            onChange={(e) => handleSensorChange(e.target.value)}
          >
            <option value="">All Sensors</option>
            {sensors.map(sensor => (
              <option key={sensor.sensorId} value={sensor.sensorId}>
                {sensor.sensorId} ({sensor.location})
              </option>
            ))}
          </select>
        </div>
      </div>

      <main className="dashboard">
        <div className="chart-section">
          <LiveChart 
            readings={readings} 
            sensorId={selectedSensor || undefined}
            maxDataPoints={200}
          />
        </div>

        <div className="panels-section">
          <StatsPanel 
            stats={stats} 
            sensors={sensors} 
            isLoading={isLoading}
          />
          
          <AlertsPanel 
            alerts={alerts} 
            isLoading={isLoading}
          />
        </div>
      </main>

      </div>
  );
}

export default App;
