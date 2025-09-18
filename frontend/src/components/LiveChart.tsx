import { useEffect, useState } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import type { SensorReading } from '../types/sensor';

interface LiveChartProps {
  readings: SensorReading[];
  sensorId?: string;
  maxDataPoints?: number;
}

export const LiveChart: React.FC<LiveChartProps> = ({ 
  readings, 
  sensorId, 
  maxDataPoints = 100 
}) => {
  const [chartData, setChartData] = useState<any[]>([]);
  const [sensors, setSensors] = useState<string[]>([]);

  useEffect(() => {
    console.log('🔍 LiveChart useEffect triggered with readings:', readings.length);
    if (!readings || readings.length === 0) {
      setChartData([]);
      setSensors([]);
      return;
    }

    // Filter readings by sensor if specified
    const filteredReadings = sensorId 
      ? readings.filter(r => r.SensorId === sensorId)
      : readings;

    // Take only the most recent readings
    const recentReadings = filteredReadings
      .sort((a, b) => new Date(a.Timestamp).getTime() - new Date(b.Timestamp).getTime())
      .slice(-maxDataPoints);

    // Get unique sensors from the filtered readings
    const sensors = [...new Set(recentReadings.map(r => r.SensorId))];
    console.log('🔍 Raw sensor IDs from readings:', recentReadings.map(r => r.SensorId).slice(0, 10));
    console.log('🔍 Unique sensors found:', sensors);
    console.log('🔍 Sample reading structure:', recentReadings[0]);
    
    // Create individual data points for each reading (no time bucketing)
    const chartData = recentReadings.map((reading, index) => {
      try {
        const timestamp = new Date(reading.Timestamp);
        const dataPoint: any = {
          time: timestamp.toLocaleTimeString(),
          timestamp: timestamp.getTime(),
          index: index
        };
        
        // Add sensor value to the data point
        dataPoint[reading.SensorId] = reading.Value;
        if (reading.IsAnomaly) {
          dataPoint[`${reading.SensorId}_anomaly`] = reading.Value;
        }
        
        return dataPoint;
      } catch (error) {
        console.error('Error processing reading:', reading, error);
        return null;
      }
    }).filter(item => item !== null);

    console.log('📊 Chart data processed:', chartData.length, 'points');
    console.log('📊 Sample chart data:', chartData.slice(0, 3));
    console.log('📊 Available sensors:', sensors);
    console.log('📊 Recent readings count:', recentReadings.length);
    console.log('📊 First few readings:', recentReadings.slice(0, 3));
    console.log('📊 Time range:', recentReadings.length > 0 ? {
      first: new Date(recentReadings[0].Timestamp).toLocaleTimeString(),
      last: new Date(recentReadings[recentReadings.length - 1].Timestamp).toLocaleTimeString()
    } : 'No readings');
    console.log('📊 Chart data sample keys:', chartData.length > 0 ? Object.keys(chartData[0]) : 'No data');
    setChartData(chartData);
    setSensors(sensors);
  }, [readings, sensorId, maxDataPoints]);

  const colors = ['#8884d8', '#82ca9d', '#ffc658', '#ff7300', '#00ff00'];

  return (
    <div style={{ width: '100%', height: '400px' }}>
      <h3>Live Sensor Data</h3>
      {chartData.length === 0 || sensors.length === 0 ? (
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center', 
          height: '300px',
          color: '#666',
          fontSize: '16px'
        }}>
          {readings.length === 0 ? 'Waiting for sensor data...' : 'Processing data...'}
        </div>
      ) : (
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 60 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis 
              dataKey="index" 
              tick={{ fontSize: 10 }}
              tickFormatter={(value, index) => {
                if (index % 20 === 0) {
                  return chartData[index]?.time || '';
                }
                return '';
              }}
              height={60}
            />
            <YAxis 
              tick={{ fontSize: 12 }}
              domain={['dataMin - 10', 'dataMax + 10']}
            />
            <Tooltip 
              formatter={(value, name) => [
                typeof value === 'number' ? value.toFixed(2) : value, 
                name
              ]}
              labelStyle={{ color: '#2c3e50' }}
              labelFormatter={(value, payload) => {
                if (payload && payload[0]) {
                  return `Time: ${payload[0].payload.time}`;
                }
                return '';
              }}
            />
            <Legend />
            {sensors.map((sensor, index) => (
              <Line
                key={sensor}
                type="monotone"
                dataKey={sensor}
                stroke={colors[index % colors.length]}
                strokeWidth={2}
                dot={false}
                connectNulls={false}
                name={sensor}
              />
            ))}
            {sensors.map((sensor, index) => (
              <Line
                key={`${sensor}_anomaly`}
                type="monotone"
                dataKey={`${sensor}_anomaly`}
                stroke={colors[index % colors.length]}
                strokeWidth={4}
                dot={{ fill: 'red', r: 4 }}
                connectNulls={false}
                name={`${sensor} (Anomaly)`}
              />
            ))}
          </LineChart>
        </ResponsiveContainer>
      )}
    </div>
  );
};
