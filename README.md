## Real-time Analytics Dashboard POC

A proof-of-concept real-time analytics dashboard that processes and displays streaming sensor data.

### Features
- **Real-time streaming**: 1000 sensor readings per second via Server-Sent Events
- **Live charts**: Multi-sensor visualization with anomaly highlighting
- **In-memory + SQLite hybrid**: 100k points in memory, 24h persistence
- **Anomaly detection**: Statistical anomaly detection with visual indicators
- **Aggregated statistics**: Real-time stats with configurable time windows
- **Alert system**: Visual alerts for anomalies and system events

### Architecture
- **Backend**: .NET 8 Web API with SQLite + in-memory ring buffer
- **Frontend**: React + TypeScript + Vite + Recharts
- **Data flow**: Sensor simulator → In-memory store → SSE stream → React dashboard
- **Persistence**: SQLite aggregates (1s buckets) with 24h auto-purge

### Prerequisites
- Node.js 20+
- .NET SDK 8 (installed locally in `~/.dotnet` by setup below)

### Install .NET SDK (user-local)
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
bash dotnet-install.sh --channel 8.0 --install-dir $HOME/.dotnet
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
```

### Quick Start

1. **Start Backend** (Terminal 1):
```bash
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
dotnet run --project Backend.Api/Backend.Api.csproj
```
- API runs on http://localhost:5172
- Swagger UI: http://localhost:5172/swagger
- Health check: http://localhost:5172/health

2. **Start Frontend** (Terminal 2):
```bash
cd frontend
VITE_API_BASE_URL=http://localhost:5172 npm run dev -- --host
```
- Dashboard: http://localhost:5173
- Network access: http://192.168.x.x:5173

### API Endpoints
- `GET /stream` - SSE stream of all sensor data
- `GET /stream/{sensorId}` - SSE stream for specific sensor
- `GET /stats?window=60s&sensorId=temp-001` - Aggregated statistics
- `GET /sensors` - List of active sensors
- `GET /alerts?limit=100` - Recent alerts
- `GET /health` - Health check

### Performance
- **Memory usage**: ~8-12MB for 100k sensor readings
- **Throughput**: 1000 readings/second sustained
- **Latency**: <100ms from generation to UI display
- **Storage**: SQLite with 1-second aggregation buckets
- **Retention**: 24-hour automatic cleanup

### Development Notes
- Sensor simulator generates realistic data with 5% anomaly rate
- Anomaly detection uses statistical z-score analysis
- Frontend maintains rolling buffer of 1000 most recent readings
- All data is UTC timestamped for consistency


