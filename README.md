# env-compliance-tracker

A full-stack application for tracking environmental compliance reports, with
role-based access and an audit trail. .NET 10 Web API backend, React + Vite
frontend.

## Run

```bash
# API (from repo root)
dotnet run --project src/EnvComplianceTracker.Api
dotnet test

# Frontend
cd client
npm install
npm run dev
```

## Configuration

Copy `appsettings.json` and provide local values via environment variables or
.NET user-secrets. The `Jwt:Key` is intentionally blank in source; set it in
your environment, and it is never committed. The default connection string uses a
local SQLite file (`compliance.db`).
