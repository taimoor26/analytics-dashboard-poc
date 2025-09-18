import type { SensorStats, Sensor, Alert } from '../types/sensor';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5172';

export class SensorApi {
  private static async fetchJson<T>(url: string): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${url}`);
    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`);
    }
    return response.json();
  }

  static async getStats(window: string = '60s', sensorId?: string): Promise<SensorStats> {
    const params = new URLSearchParams({ window });
    if (sensorId) params.append('sensorId', sensorId);
    return this.fetchJson<SensorStats>(`/stats?${params}`);
  }

  static async getSensors(): Promise<Sensor[]> {
    return this.fetchJson<Sensor[]>('/sensors');
  }

  static async getAlerts(limit: number = 100): Promise<Alert[]> {
    return this.fetchJson<Alert[]>(`/alerts?limit=${limit}`);
  }

  static createEventSource(sensorId?: string): EventSource {
    const url = sensorId ? `/stream/${sensorId}` : '/stream';
    return new EventSource(`${API_BASE_URL}${url}`);
  }
}
