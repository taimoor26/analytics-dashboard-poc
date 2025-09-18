

# AI Conversation Log – Real-time Analytics Dashboard POC

This document contains the exported Cursor conversation and a short summary of key disagreements and decisions.

## Full Conversation
See attached file [`cursor_building_a_react_app_with_net_co.md`](./cursor_building_a_react_app_with_net_co.md).

## Key Disagreements With AI
1. **Memory vs. SQLite:** Cursor initially proposed an in-memory store only. We decided on a hybrid: 100 k points in memory + SQLite aggregates for 24 h retention.
2. **Transport Protocol:** Cursor proposed WebSockets; we selected **Server-Sent Events (SSE)** for simpler one-way streaming.
3. **Aggregation Strategy:** AI first suggested inserting raw readings into SQLite (1 000/sec × 24 h ≈ 86 million rows). We implemented 1-second aggregated buckets instead.

### Example of Avoided Performance Issue
Had we stored raw readings in SQLite as suggested initially, the database would have grown to ~86 million rows/day.  
Using aggregates instead keeps it to ~86 000 rows/day and allows fast queries.

---