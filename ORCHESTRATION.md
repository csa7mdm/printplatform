# Multi-Agent Orchestration Contract

This repo is built by **multiple AI agents in parallel** (Claude, Codex, Gemini). To let
independent agents work simultaneously without trampling each other, every agent MUST follow
this contract. The goal: **modules touch disjoint files**, so there are no edit-time races and
near-zero merge conflicts.

## Roles
- **Claude (main loop)** — orchestrator. Owns the architecture baseline, branch assignment,
  merges, the `dotnet build` + test gate, conflict resolution, and final review. Does NOT
  write bulk module code while delegating.
- **Codex (gpt-5.5)** — independent backend slices + adversarial test generation.
- **Gemini** — frontends + independent slices (rate-limited on free tier; use when available).
- **Claude subagents (workflow)** — coupling-critical logic (ledger atomicity, dispatch
  concurrency) when the session limit allows.

## The conflict-free architecture (Layer 1)
Shared files are designed out of existence via auto-registration. Agents NEVER edit these:

| File | Mechanism | Per-module action |
|------|-----------|-------------------|
| `PrintPlatform.API/Program.cs` | scans assemblies for `IModuleInstaller` and invokes all | none — frozen |
| `Infrastructure/Data/AppDbContext.cs` | `partial class` + `modelBuilder.ApplyConfigurationsFromAssembly(...)` | add your own `AppDbContext.<Module>.cs` partial with your `DbSet<>`s |
| MediatR / FluentValidation registration | assembly scan in the module installer | none |

Each module is a **vertical slice**. A module owns ONLY files under:
- `Domain/<Module>/...`
- `Application/<Module>/...`
- `Infrastructure/<Module>/...` (incl. its `IEntityTypeConfiguration<T>` files)
- `Infrastructure/Data/AppDbContext.<Module>.cs` (its own partial — DbSets)
- `API/Controllers/<Module>Controller.cs`
- one `Infrastructure/<Module>/<Module>ModuleInstaller.cs` implementing `IModuleInstaller`

## Rules for every agent
1. **Read before write.** Inspect the completed **Marketplace** module as the reference
   pattern, and any partially-built files for your module. COMPLETE; never duplicate a type.
2. **Stay in your slice.** Only create/edit files in your module's folders (above).
3. **Never edit a shared file.** If you think you need to touch `Program.cs`, `AppDbContext.cs`,
   `.sln`, or another module's files — STOP. Use your `IModuleInstaller` + your partial DbContext.
4. **`.csproj` edits are additive only** (append a `<PackageReference>`; never reorder/remove).
5. **Definition of done:** your branch builds (`dotnet build` from repo root succeeds) and your
   module's unit tests pass. State this explicitly in your final summary.
6. **Output** the list of files created/edited + a one-paragraph summary.

## Git workflow (Layer 2)
- Each unit gets a branch `feat/<unit>` in its own worktree under `../pp-worktrees/<unit>`
  so agents are filesystem-isolated.
- The orchestrator merges green branches sequentially into `main`, running the build+test gate
  after each. Residual conflicts (rare) are `.csproj` unions, resolved by the orchestrator.
- Worktree create: `git worktree add ../pp-worktrees/<unit> -b feat/<unit>`
- Worktree remove after merge: `git worktree remove ../pp-worktrees/<unit>`

## Current status (living)
- ✅ scaffold, Gamification pkg, Loyalty pkg, Marketplace module, 3 frontends
- ⏳ Identity, Orders (partial), Dispatch, Finance, Integrations — to finish under this contract
- ⏳ Wire & Verify (event handlers, migrations, integration tests)
- ⏳ Layer-1 refactor (IModuleInstaller + partial AppDbContext) — orchestrator, on main first
