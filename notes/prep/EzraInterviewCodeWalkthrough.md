# "Walk me through your code" — Bulleted Response

**Use this as a spoken outline.** ~10–15 min. Have repo open; jump to files as you go. Pause for questions.

---

## Opener (30 sec)

- Minimal todo app — API + React frontend, production-minded MVP not a throwaway demo
- Decoupled SPA + ASP.NET Core API, cookie auth, OpenAPI contract, tests, Docker, CI
- Tradeoffs and gaps documented in [DESIGN.md](../DESIGN.md)

---

## Architecture (1 min)

- **Browser:** React + Vite SPA; TanStack Router + TanStack Query
- **Wire:** HTTP/JSON; `credentials: 'include'` sends Identity session cookie on every request
- **API:** ASP.NET Core — Identity, REST controllers, Serilog, global exception handler
- **Data:** EF Core → SQLite in-memory (demo; Postgres path documented)
- **Dev:** split processes (API `:5162`, Vite `:5173` with proxy) · **Prod:** one Docker container, API serves SPA on `:8080`

---

## Backend bootstrap — `Program.cs`

*Bootstrap = how the app starts up and wires itself together before it can handle requests.*

- SQLite in-memory — connection kept open for app lifetime
- Identity API endpoints + explicit `/logout` (server-side session invalidation)
- CORS with credentials for Vite dev origin
- Middleware order: exception handler → auth → Serilog request logging (enriched with `UserId`)
- `EnsureCreated()` at startup — no migrations in MVP
- Production: static files + SPA fallback to `index.html`

---

## Auth & multi-tenant isolation — `EntryController.cs`

- **`[Authorize]` on the class** — secure default; new endpoints can't ship without auth
- **`UserId` from `ClaimTypes.NameIdentifier`** — never from request body
- **Every query scoped:** `WHERE UserId == userId AND Status != Inactive`
- **404 not 403** for cross-user access — don't confirm another user's row exists
- **Create:** stamps `UserId` from claim
- **Update/Delete:** same ownership query; Warning log on rejected cross-user mutations
- **Delete:** soft delete (`Inactive`) — row kept for future trash/restore
- **Early bug I fixed:** all users could see all entries → made scoping non-negotiable + tests

---

## Settings — `SettingsController.cs`

- Per-user font preference; same auth/ownership pattern as entries
- **Lazy-create on first GET** — no settings row at registration; created with default font when first fetched
- **Font allowlist** on PUT — server validates even if UI only offers known fonts
- Unique index on `Settings.UserId` in `AppDbContext`

---

## Errors & logging

- **`GlobalExceptionHandler.cs`** — unhandled exceptions → log full error, return RFC 7807 ProblemDetails (no stack trace to client)
- **Serilog** — structured request logs with `UserId` when authenticated

---

## Data model — `Models/`

- **`Entry`** — flat table + `EntryKind` enum (Todo/Note/Event); only Todo in UI today
- **`EntryStatus`** — Active → InProgress → Completed → Archived; `Inactive` = deleted
- **`EntryRequests`** — no `UserId` field by design
- **`User`** — thin Identity user class

---

## API contract — OpenAPI / generated client

- Swagger on API → `pnpm generate-api` → typed client in `src/api/generated/`
- Single source of truth — frontend types stay in sync with backend

---

## Frontend — `main.tsx`, routes, hooks

- **`main.tsx`** — QueryClientProvider, Chakra, generated fetch client with `credentials: 'include'`
- **`routes/index.tsx`** — `beforeLoad` checks auth; redirects to `/login` if not signed in
- **`useAuth.ts`** — auth as React Query cache; login uses `useCookies: true`
- **`useEntries.ts`** — create/update/delete mutations; optimistic updates with rollback on error
- **`crossTabSync.ts`** — BroadcastChannel + window focus refetch for pop-out window (last-write-wins)

---

## UX choices (if time)

- Composer at top — Enter to add, minimal clicks
- Status cycles forward only — no accidental undo of "done"
- InProgress sorts top, Archived bottom (client-side for MVP)
- Pop-out ~440×720 side panel instead of resizing browser
- Settings popover stays open on desktop so user sees font change live

---

## Tests — `tests/` + `*.test.tsx`

- **65 API tests** — Entry CRUD, authorization (cross-user 404), validation, settings, exception handler, auth integration (Bearer), CORS
- **17 frontend tests** — optimistic rollback, composer, auth errors
- **Gap I'd name:** integration tests use Bearer; SPA uses cookies — Playwright E2E is next step

---

## Ops — CI & Docker

- **`.github/workflows/ci.yml`** — API tests, web tests + build, Docker build (parallel)
- **`Dockerfile`** — multi-stage: Node build → .NET publish → single port

---

## Honest gaps (say before they find them)

- In-memory DB — no durability across restart
- No pagination, virtualization, or performance testing
- No Redis session store for multi-instance
- Known edge case: Completed → Archived in pop-out (DESIGN.md §8)

---

## Close (15 sec)

- "Happy to go deeper anywhere — security, optimistic updates, tradeoffs, or what I'd change with another week."
- **Files ready to open:** `EntryController.cs`, `useEntries.ts`, `AuthorizationTests.cs`, `DESIGN.md`

---

**Related:** [EzraInterviewCodeTour.md](EzraInterviewCodeTour.md) · [EzraInterviewNarrative.md](EzraInterviewNarrative.md) · [EzraPanelInterviewOutline.md](EzraPanelInterviewOutline.md)
