# env-compliance-tracker

A full-stack application for tracking environmental compliance reports, with
role-based access and an audit trail. .NET 10 Web API backend, React + Vite
frontend.

## Architecture

Clean-architecture layering across three projects:

- **`src/EnvComplianceTracker.Domain`**: entities and domain logic
- **`src/EnvComplianceTracker.Infrastructure`**: EF Core (`ComplianceDbContext`),
  password hashing, data seeding
- **`src/EnvComplianceTracker.Api`**: ASP.NET Core Web API, JWT auth, role
  enforcement, current-user resolution

The frontend lives in **`client/`** (React 19 + Vite). Tests are in
**`tests/EnvComplianceTracker.Api.Tests`** (xUnit + `WebApplicationFactory`),
covering reports, auth/roles, and the audit trail.

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
