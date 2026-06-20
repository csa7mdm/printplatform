# PrintPlatform — Egypt 3D Printing Fulfillment Platform

A trusted **aggregation & fulfillment layer for 3D printing in Egypt**: a vetted network of printer
owners, operator-driven QC, B2B SLAs, and a single invoice. Two-sided marketplace — **customers** upload
models and order prints; **printer owners** earn passive income fulfilling jobs; **operators** review
quotes, dispatch jobs, and run QC.

> **Positioning:** "both, sequenced" — B2B accounts + design/CAD services as the Phase 1 cash engine,
> distributed-network SLA as the Phase 2 moat once volume exceeds a single farm.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/license-Proprietary-lightgrey)](#license)

---

## Table of Contents
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Repository Layout](#repository-layout)
- [Getting Started](#getting-started)
- [Running the Tests](#running-the-tests)
- [API Surface](#api-surface)
- [Conflict-Free Modular Design](#conflict-free-modular-design)
- [Project Status](#project-status)
- [Roadmap & Contributing](#roadmap--contributing)
- [For AI Agents](#for-ai-agents)

---

## Architecture

A **modular monolith** on ASP.NET Core 10, organized as vertical slices over a clean-architecture spine
(`Domain` → `Application` → `Infrastructure` → `API`). Business modules:

| Module | Responsibility |
|--------|----------------|
| **Identity** | Two-sided auth (Customer / PrinterOwner / Operator / Admin), JWT + refresh tokens, WhatsApp OTP, encrypted NationalId/bank details |
| **Marketplace** | Printer fleet, materials catalog, ranked dispatch candidate scoring |
| **Orders** | Model upload (MinIO), instant estimate, operator quoting, order + payment lifecycle |
| **Dispatch** | Job offer/accept/print/QC/ship state machine, re-routing, Bosta shipping |
| **Finance** | Append-only immutable ledger, weekly InstaPay/bank payouts |
| **Integrations** | WhatsApp Business API, MinIO storage, health checks, global exception handling |

Two **reusable packages with zero domain coupling** (operate only on a string `ExternalUserId`):

- **`PrintPlatform.Gamification`** — XP, levels, achievements, streaks, leaderboards (own DbContext, `gam_` tables)
- **`PrintPlatform.Loyalty`** — tiers, append-only points ledger, rewards, referrals (own DbContext, `loy_` tables)

Cross-module communication is via **domain events** (MediatR-free `IDomainEvent` wrapped in
`DomainEventNotification<T>`); handlers return `Result<T>` (no exceptions for business logic).

## Tech Stack

**Backend** — ASP.NET Core 10 · EF Core 10 · PostgreSQL 16 · MediatR · FluentValidation · Mapster ·
Hangfire (background jobs) · Serilog · OpenAPI via `Microsoft.AspNetCore.OpenApi` + **Scalar** (no Swashbuckle) ·
HTTP resilience via `Microsoft.Extensions.Http.Resilience` (no Polly.Http) · MinIO (S3) · ASP.NET Data Protection

**Integrations** — Paymob (payments) · Bosta (shipping) · WhatsApp Business API (notifications/OTP)

**Frontend** — React 18 · TypeScript · Vite · TanStack Query v5 · Tailwind CSS · i18next (Arabic RTL + English LTR) ·
three.js (STL preview) · React Hook Form + Zod. Three apps: **customer**, **operator**, **printer-owner**.

**Infra** — Docker / docker-compose · GitHub Actions CI · Testcontainers (integration tests)

## Repository Layout

```
src/
  PrintPlatform.API/             ASP.NET Core entry point, controllers, Program.cs
  PrintPlatform.Application/     MediatR commands/queries/handlers, DTOs, abstractions
  PrintPlatform.Domain/          Entities, value objects, domain events, Result<T>
  PrintPlatform.Infrastructure/  EF Core, adapters (Paymob/Bosta/WhatsApp/MinIO), module installers
  packages/
    PrintPlatform.Gamification/  Reusable engine (no domain refs)
    PrintPlatform.Loyalty/       Reusable engine (no domain refs)
tests/                           Domain + Application (Testcontainers integration) tests
frontend/
  customer/  operator/  printer-owner/    Three Vite + React 18 apps
docs/                            openapi.json, DEPLOY.md
ORCHESTRATION.md                 Multi-agent build contract (how to add a module conflict-free)
docker-compose.yml  Dockerfile  Makefile  .github/workflows/ci.yml
```

## Getting Started

### Prerequisites
- .NET 10 SDK · Node 18+ · Docker Desktop · (optional) `dotnet-ef` tool: `dotnet tool install -g dotnet-ef`

### 1. Backend via Docker (recommended)
```bash
cp .env.example .env            # fill in secrets
docker compose up -d postgres minio
dotnet ef database update --context AppDbContext \
  --project src/PrintPlatform.Infrastructure --startup-project src/PrintPlatform.API
# (repeat database update for GamificationDbContext, LoyaltyDbContext, MarketplaceDbContext)
dotnet run --project src/PrintPlatform.API
```
API: `http://localhost:8080` · OpenAPI/Scalar UI: `/scalar` · Health: `/health/ready` · Hangfire: `/hangfire`

### 2. Frontends
```bash
cd frontend/customer && npm install && npm run dev    # also: operator, printer-owner
```
Each reads `VITE_API_URL` (defaults to the local API).

> **Note:** the API auto-seeds reference data (materials, gamification levels, loyalty tiers, admin) at
> startup outside the `Testing` environment — apply migrations first.

## Running the Tests

```bash
dotnet test PrintPlatform.sln        # requires Docker running (Testcontainers spins up postgres:16)
```

Integration tests are **host-less**: they build a service provider from the app's real registration
extensions against a Testcontainers Postgres, exercising the full DI graph + EF + seeding end-to-end.

## API Surface

`Auth` · `Profile` · `Models` · `Quotes` · `Orders` · `Payments` (Paymob webhook) · `Printers` ·
`Materials` · `Dispatch` (offer/accept/print/QC) · `Finance` (earnings/payouts/ledger) · `Achievements` ·
`Loyalty` · `Webhooks` (WhatsApp/Bosta). Full spec at `docs/openapi.json` and the Scalar UI.

## Conflict-Free Modular Design

Modules **self-register** so adding one never edits a shared file (enables parallel development):

- **DI:** each module ships an `IModuleInstaller` discovered by assembly scan — `AddInfrastructure` is frozen.
- **Persistence:** `AppDbContext` is `partial`; each module owns `AppDbContext.<Module>.cs` (DbSets) +
  `IEntityTypeConfiguration<T>` files auto-applied via `ApplyConfigurationsFromAssembly`.
- **Handlers/validators:** auto-registered via MediatR / FluentValidation assembly scanning.

See **[ORCHESTRATION.md](ORCHESTRATION.md)** for the step-by-step "how to add a module" contract.

## Project Status

✅ Solution builds clean (0 errors) · ✅ Full test suite green · ✅ App boots with a validated DI graph ·
✅ All modules + full REST API · ✅ EF migrations for all 4 DbContexts · ✅ 3 frontends with typed,
route-reconciled API clients · ✅ No vulnerable packages / version conflicts.

See **[GitHub Issues](../../issues)** and the **[Project board](../../projects)** for the live backlog.

## Roadmap & Contributing

High-value next steps are tracked as GitHub Issues (labelled `good-first-issue`, `backend`, `frontend`,
`enhancement`). Highlights: real STL geometry parsing, printer-owner page wiring, HTTP-level auth E2E
tests, Hangfire dashboard authorization, production migration/seeding strategy, real Paymob/Bosta/WhatsApp
integration testing.

PRs welcome. Each change should keep `dotnet build` + `dotnet test` green and follow the
[ORCHESTRATION.md](ORCHESTRATION.md) module contract.

## For AI Agents

This repo is designed for autonomous agents to pick up from latest:
1. Read **[ORCHESTRATION.md](ORCHESTRATION.md)** (module contract + conventions) and this README's
   [Conflict-Free Modular Design](#conflict-free-modular-design).
2. Check open **[Issues](../../issues)** / the **[Project board](../../projects)** for the next task.
3. Work in a git worktree / branch; keep to your module's slice; build + test green before opening a PR.
4. Conventions: handlers return `Result<T>`; domain events are MediatR-free and consumed via
   `DomainEventNotification<T>`; money is `decimal` EGP; enums persist as strings.

## License

Proprietary — all rights reserved (pending). Contact the maintainer before reuse.
