# Real-time Analytics Dashboard - Local Setup Guide

This guide will help you set up and run the Real-time Analytics Dashboard locally on your machine.

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

### Required Software
- **Node.js** (version 20.19+ or 22.12+)
  - Download from [nodejs.org](https://nodejs.org/)
  - Verify installation: `node --version`
- **.NET 8 SDK**
  - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
  - Verify installation: `dotnet --version`
- **Git** (for cloning the repository)
  - Download from [git-scm.com](https://git-scm.com/)

### Optional but Recommended
- **Visual Studio Code** with C# and TypeScript extensions
- **Postman** or similar API testing tool

## 🚀 Quick Start

### 1. Clone the Repository
```bash
git clone <your-github-repo-url>
cd <repository-name>
```

### 2. Backend Setup (.NET Core API)

Navigate to the backend directory:
```bash
cd Backend.Api
```

Restore dependencies:
```bash
dotnet restore
```

Run the backend:
```bash
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5172`
- **HTTPS**: `https://localhost:7172`
- **Swagger UI**: `https://localhost:7172/swagger`

### 3. Frontend Setup (React + Vite)

Open a new terminal and navigate to the frontend directory:
```bash
cd frontend
```

Install dependencies:
```bash
npm install
```

Set the API base URL and run the frontend:
```bash
VITE_API_BASE_URL=http://localhost:5172 npm run dev
```

The frontend will be available at:
- **Local**: `http://localhost:5173`
- **Network**: `http://[your-ip]:5173`

## 🔧 Detailed Setup Instructions

### Backend Configuration

The backend uses:
- **.NET 8** Web API
- **SQLite** database (auto-created on first run)
- **Entity Framework Core** for data access
- **Server-Sent Events (SSE)** for real-time streaming

#### Database
The SQLite database (`analytics.db`) will be automatically created in the `Backend.Api` directory on first run. No additional setup required.

#### Environment Variables
The backend uses default configuration. To customize:
1. Create `appsettings.Development.json` in `Backend.Api` directory
2. Add your custom settings

### Frontend Configuration

The frontend uses:
- **React 18** with TypeScript
- **Vite** as build tool
- **Recharts** for data visualization
- **EventSource API** for real-time data

#### Environment Variables
Create `.env.local` in the `frontend` directory:
```env
VITE_API_BASE_URL=http://localhost:5172
```

## 🏗️ Project Structure

```
├── Backend.Api/                 # .NET Core Web API
│   ├── Controllers/            # API Controllers
│   ├── Data/                   # Entity Framework DbContext
│   ├── Endpoints/              # API Endpoints
│   ├── Models/                 # Data Models
│   ├── Services/               # Business Logic Services
│   ├── Program.cs              # Application Entry Point
│   └── Backend.Api.csproj     # Project File
├── frontend/                   # React Frontend
│   ├── src/
│   │   ├── components/         # React Components
│   │   ├── api/               # API Client
│   │   ├── types/             # TypeScript Types
│   │   ├── App.tsx            # Main App Component
│   │   └── main.tsx           # Entry Point
│   ├── package.json           # Dependencies
│   └── vite.config.ts         # Vite Configuration
├── README.md                   # Project Overview
└── setup.md                   # This Setup Guide
```

## 🚦 Running the Application

### Start Backend (Terminal 1)
```bash
cd Backend.Api
dotnet run
```

### Start Frontend (Terminal 2)
```bash
cd frontend
VITE_API_BASE_URL=http://localhost:5172 npm run dev
```

### Access the Application
1. Open your browser
2. Navigate to `http://localhost:5173`
3. You should see the Real-time Analytics Dashboard

## 🔍 Features Overview

### Real-time Data Streaming
- **1000+ sensor readings per second**
- **Server-Sent Events (SSE)** for live updates
- **5 different sensor types** (temperature, humidity, pressure, vibration)

### Data Visualization
- **Live line charts** showing sensor data over time
- **Anomaly detection** with visual indicators
- **Real-time statistics** and aggregations

### Data Management
- **Hybrid memory system**:
  - In-memory ring buffer for hot data (100k points)
  - SQLite database for persistent storage (24h retention)
- **Automatic data aggregation** (1-second buckets)
- **Anomaly detection** and alerting

## 🐛 Troubleshooting

### Common Issues

#### Backend Issues
1. **Port already in use**
   ```bash
   # Kill process using port 5172
   lsof -ti:5172 | xargs kill -9
   ```

2. **Database connection issues**
   ```bash
   # Delete and recreate database
   rm analytics.db
   dotnet run
   ```

3. **Missing dependencies**
   ```bash
   dotnet restore
   dotnet build
   ```

#### Frontend Issues
1. **Node.js version warning**
   - Upgrade to Node.js 20.19+ or 22.12+
   - Or use `--legacy-peer-deps` flag: `npm install --legacy-peer-deps`

2. **API connection issues**
   - Verify backend is running on `http://localhost:5172`
   - Check CORS settings in backend
   - Verify `VITE_API_BASE_URL` environment variable

3. **Chart not displaying**
   - Check browser console for errors
   - Verify data is being received from API
   - Check network tab for SSE connection

#### General Issues
1. **CORS errors**
   - Backend is configured for `http://localhost:5173`
   - If using different port, update CORS in `Backend.Api/Program.cs`

2. **Memory issues**
   - The app is designed to handle high data volumes
   - In-memory buffer automatically manages memory usage
   - Database cleanup runs automatically every hour

## 📊 API Endpoints

### Streaming Endpoints
- `GET /stream` - Stream all sensor data
- `GET /stream/{sensorId}` - Stream specific sensor data

### Statistics Endpoints
- `GET /stats?window=60s&sensorId={id}` - Get aggregated statistics
- `GET /sensors` - Get sensor information
- `GET /alerts?limit=50` - Get recent alerts

### Health Check
- `GET /health` - API health status

## 🔧 Development

### Adding New Features

#### Backend
1. Add new models in `Backend.Api/Models/`
2. Create endpoints in `Backend.Api/Endpoints/`
3. Add services in `Backend.Api/Services/`
4. Update `Program.cs` to register new services

#### Frontend
1. Add new components in `frontend/src/components/`
2. Update types in `frontend/src/types/`
3. Add API methods in `frontend/src/api/`
4. Update main app in `frontend/src/App.tsx`

### Database Schema
- **SensorAggregates**: Time-bucketed sensor data
- **SensorInfo**: Current sensor status
- **Alerts**: Anomaly alerts and notifications

## 📝 Notes

- The application simulates sensor data for demonstration purposes
- Data is automatically cleaned up after 24 hours
- The system is designed to handle high-frequency data streams
- All timestamps are in UTC

## 🆘 Support

If you encounter issues:
1. Check the troubleshooting section above
2. Verify all prerequisites are installed
3. Check the console logs for error messages
4. Ensure both backend and frontend are running
5. Verify network connectivity between frontend and backend

## 📄 License

This project is for demonstration purposes. Please check the main repository for licensing information.
