# Roadmap

Live backlog: **[GitHub Issues](../../issues)** (milestone **v1.0 — Production Readiness**).
This file mirrors it so the plan is readable from the repo (useful for offline work and AI agents).

> Pick the lowest-numbered unblocked item, work it in a branch/worktree within its module slice,
> keep `dotnet build` + `dotnet test` green, open a PR. See [ORCHESTRATION.md](ORCHESTRATION.md).

## v1.0 — Production Readiness

| # | Title | Area |
|---|-------|------|
| 1 | Implement real STL geometry parsing (`ModelGeometryAnalysisJob` is a placeholder) | backend |
| 2 | Wire printer-owner pages to the API hooks (only the API layer exists) | frontend |
| 3 | Re-reconcile frontend API routes to now-existing controllers (clear stale `TODO(backend)`) | frontend |
| 4 | Add HTTP-level integration tests (auth + controllers) | testing |
| 5 | Implement the full OrderFlow E2E test (currently a seeder-check stub) | testing |
| 6 | Add Hangfire dashboard authorization filter | security |
| 7 | Production migration & seeding strategy (app seeds at startup today) | devops |
| 8 | Real Paymob integration + sandbox testing | backend / integration |
| 9 | Real Bosta shipping integration | backend / integration |
| 10 | WhatsApp Business API integration + message templates | backend / integration |
| 11 | Verify CI pipeline with Testcontainers (Docker-in-CI) | devops |
| 12 | STL 3D preview in customer/operator apps (three.js) | frontend |

## Done (recent)
- Modular monolith + all modules + full REST API; Gamification & Loyalty packages.
- Conflict-free auto-registration (`IModuleInstaller`, partial `AppDbContext`).
- EF migrations for all 4 DbContexts; host-less Testcontainers integration tests (green).
- App boots with a fully validated DI graph; 0 build warnings; no vulnerable packages / version conflicts.
