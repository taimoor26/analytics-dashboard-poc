

# Decision Document – Real-time Analytics Dashboard POC

## Architecture Decisions & Trade-offs
- **Backend:** .NET 8 Web API chosen for high-performance background services.
- **Frontend:** React + TypeScript + Vite for rapid development and hot-module reload.
- **Streaming Transport:** Server-Sent Events (SSE) over WebSockets for simple, one-way push of sensor data.
- **Data Model:** In-memory ring buffer for 100 k hot points; SQLite for 24 h aggregates.

## Why Hybrid Memory + SQLite
Cursor initially proposed in-memory only. At 1 000 readings/sec × 24 h = 86 million rows, memory alone is infeasible.  
We implemented:
- 100 k points in memory for real-time charts and anomaly detection.
- 1-second aggregated buckets in SQLite for 24 h retention with auto-purge.

## Performance Optimizations
- Fixed-size ring buffer with lock + `ConcurrentQueue` to bound memory.
- Batched writes to SQLite every 10 s to reduce I/O.
- PRAGMAs for WAL and memory caching.
- Frontend keeps only last 1 000 readings per sensor to stay responsive.

## AI Suggestions Rejected
- Storing all raw data in SQLite (would bloat DB).
- Using only in-memory store (no persistence).
- Defaulting to WebSockets (overkill for one-way push).

## Validation at Scale
- Simulated 1 000 readings/sec with `SensorSimulatorService`.
- Verified memory stays ~8–12 MB for 100 k readings.
- Checked SQLite grows only 86 k rows/day (1-second buckets).
- Observed latency <100 ms from generation to UI.

---