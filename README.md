# PrintPlatform

PrintPlatform is a work-in-progress 3D printing fulfillment platform for Egypt. The codebase models a two-sided workflow where customers upload print jobs, printer owners register machines and materials, and operators manage quoting, dispatch, quality control, payments, and payouts.

The repository is structured as a modular ASP.NET Core backend with three React frontends.

## What Is In This Repository

- ASP.NET Core API targeting `.NET 10`
- Clean architecture style backend split into `Domain`, `Application`, `Infrastructure`, and `API`
- PostgreSQL persistence through Entity Framework Core
- JWT authentication and refresh token flow
- Controllers for auth, profile, model upload, quotes, orders, dispatch, finance, loyalty, achievements, materials, printers, payments, and webhooks
- Health checks for liveness and readiness
- OpenAPI generation with Scalar UI
- Hangfire registration for background processing
- MinIO/S3 style object storage integration points
- Three Vite + React frontends:
  - `frontend/customer`
  - `frontend/operator`
  - `frontend/printer-owner`
- Reusable gamification and loyalty packages under `src/packages`
- xUnit test projects for domain, application, gamification, and loyalty behavior

## Current Status

This is an active product codebase, not a packaged SaaS release. The core backend modules are present and the solution includes tests, migrations, API controllers, and frontend shells. Some integrations depend on external services and real credentials before they can be exercised end to end.

Known areas that still need production hardening:

- Real payment, shipping, and messaging credentials
- Full operator and printer-owner frontend wiring against every backend route
- Deployment-specific migration and secret management
- End-to-end HTTP tests for authenticated workflows
- Authorization policies around operational dashboards such as Hangfire

## Domain Model

The platform is organized around these business areas:

| Area | Purpose |
| --- | --- |
| Identity | Customer and printer-owner registration, login, refresh tokens, OTP verification, profile onboarding |
| Marketplace | Printer registration, materials, availability, printer verification, dispatch candidates |
| Models and Quotes | Model upload, quote requests, operator quote confirmation, customer quote acceptance |
| Orders | Order listing, order detail, payment initiation, cancellation |
| Dispatch | Job assignment, acceptance, rejection, print start, completion photos, QC approval/rejection |
| Finance | Customer payment records, refunds, owner earnings, payout records, ledger views |
| Loyalty | Member records, tiers, rewards, points ledger, referrals |
| Gamification | Player records, XP, levels, achievements, challenges, streaks, leaderboards |
| Webhooks | WhatsApp verification/callbacks and payment webhook entry points |

## API Surface

The API maps controllers under these route groups:

- `/api/auth`
- `/api/profile`
- `/api/models`
- `/api/quotes`
- `/api/orders`
- `/api/dispatch`
- `/api/finance`
- `/api/printers`
- `/api/materials`
- `/api/payments`
- `/api/webhooks`
- `/api/loyalty`
- `/api/achievements`
- `/health/live`
- `/health/ready`

OpenAPI is enabled in development through the ASP.NET Core OpenAPI/Scalar setup.

## Architecture

```text
src/
  PrintPlatform.API/             ASP.NET Core host and controllers
  PrintPlatform.Application/     Commands, queries, DTOs, handlers, validation
  PrintPlatform.Domain/          Entities, value objects, domain events, Result type
  PrintPlatform.Infrastructure/  EF Core, integrations, background jobs, module registration
  packages/
    PrintPlatform.Gamification/  Reusable gamification package
    PrintPlatform.Loyalty/       Reusable loyalty package

tests/
  PrintPlatform.Domain.Tests/
  PrintPlatform.Application.Tests/
  PrintPlatform.Gamification.Tests/
  PrintPlatform.Loyalty.Tests/

frontend/
  customer/
  operator/
  printer-owner/
```

The backend uses a modular monolith approach. Modules own their handlers, persistence configuration, and registration where possible. Domain events are kept independent from MediatR and are wrapped at the application boundary.

## Technology

Backend:

- .NET 10
- ASP.NET Core
- Entity Framework Core 10
- PostgreSQL
- MediatR
- FluentValidation
- Mapster
- Hangfire
- Serilog
- Scalar/OpenAPI
- MinIO client
- xUnit, FluentAssertions, NSubstitute, Testcontainers

Frontend:

- React 18
- TypeScript
- Vite
- TanStack Query
- Tailwind CSS
- i18next
- React Hook Form
- Zod
- three.js for model preview surfaces

Infrastructure:

- Docker Compose
- PostgreSQL
- MinIO

## Getting Started

Prerequisites:

- .NET 10 SDK
- Docker Desktop
- Node.js 18 or newer
- npm

Start local infrastructure:

```bash
docker compose up -d postgres minio
```

Run the backend:

```bash
dotnet restore
dotnet run --project src/PrintPlatform.API
```

Run tests:

```bash
dotnet test PrintPlatform.sln
```

Run a frontend:

```bash
cd frontend/customer
npm install
npm run dev
```

Repeat the same pattern for `frontend/operator` and `frontend/printer-owner`.

## Configuration

Use `.env.example` as the starting point for local settings. The API expects database, storage, JWT, and integration settings to be supplied through configuration.

Do not commit real secrets. Payment, shipping, and messaging integrations should be configured through local environment variables or the deployment platform secret store.

## Notes For Reviewers

This repository is useful for reviewing:

- Modular monolith structure in ASP.NET Core
- Clean architecture boundaries in a real domain
- EF Core persistence split across modules
- Marketplace/order/dispatch/finance workflow modeling
- Separate frontend shells for different user roles
- Reusable loyalty and gamification packages

It should not be read as a turnkey production deployment without completing the external integration and deployment work listed above.
