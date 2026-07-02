# Ezra Interview — 2-Min Narrative + 10-Min Walkthrough

**Practice out loud.** Target: 2 minutes for opener, ~10 minutes for walkthrough (they may go deeper on any section).

---

# #$@#$@#$@#$@#$@#

REWRITE

## 2-Minute Core Narrative (script)

> I built a minimal todo app as a **production-minded MVP**, not a prototype. The assignment asked for API and frontend; I interpreted that as real auth, multi-tenant data isolation, tests, Docker, CI, and structured logging — ~~because in healthtech, demo quality isn't enough when the product touches people's lives~~.
>
> I hadn't touched C# in five years — mostly Python at Zapier — so I **reset once** when AI-generated scaffolding got ahead of my ability to review it, then rebuilt with small, reviewable changes. That mirrors how I'd ramp on Ezra's .NET stack: learn fast, but never ship what I can't explain.
>
> Architecturally: decoupled React SPA plus ASP.NET Core API, **cookie sessions via Identity** for real server-side logout, OpenAPI codegen for type safety, TanStack Query for optimistic UX, SQLite in-memory for the demo with a documented path to Postgres and pagination.
>
> I also optimized for **minimal clicks** — composer at top, inline edit, forward-only status lifecycle — because my clinical and front-desk experience taught me that friction kills adoption in health workflows. Tradeoffs and gaps are documented in DESIGN.md.

---

# #$@#$@#$@#$@#$@#

SMALLER EDITS

## 10-Minute Walkthrough (section-by-section)

### §1 — Problem interpretation (~1 min)

**Assignment:** "Build a small to-do task management API and frontend."

**My read:**

- CRUD entries with a status lifecycle (Active → InProgress → Completed → Archived; Inactive = soft delete)
- Auth so data is per-user
- Production hygiene: tests, Docker, CI, exception handling, logging

**Product lens:** Todo apps are too complicated. Goal = minimal clicks — type and Enter to add, click to edit inline, one click to advance status.

**Reference:** [DESIGN.md §1](DESIGN.md)

---

### §2 — Architecture overview (~1.5 min)

```
Browser (React + Vite)
    ↓ credentials: 'include' (cookies)
ASP.NET Core API (Identity, controllers)
    ↓ EF Core
SQLite in-memory
```

**Backend** — [Program.cs](../src/todostodo.api/Program.cs):

- ASP.NET Identity API (`/login`, `/register`, `/logout`)
- Serilog request logging (UserId enriched)
- Global exception handler → RFC 7807 ProblemDetails
- CORS whitelist with credentials

**Frontend** — [main.tsx](../src/todostodo.web/src/main.tsx):

- OpenAPI → `pnpm generate-api` → typed client in `src/api/generated/`
- TanStack Query for all server state (auth, entries, settings)
- TanStack Router with auth guard on `/`

**Dev vs prod:** Dev = split processes (API :5162, Vite :5173 with proxy). Prod = single Docker container, API serves SPA on :8080.

---

### §3 — Security and data isolation (~2 min) ★ Know this cold

**The bug I fixed:** Early build returned all users' entries. Made UserId scoping non-negotiable.

**Patterns in** [EntryController.cs](../src/todostodo.api/Controllers/EntryController.cs):

1. **Class-level `[Authorize]`** — secure default; new endpoints can't ship without auth
2. **UserId from claim only** — `ClaimTypes.NameIdentifier`, never from request body
3. **Every query scoped:** `WHERE UserId == userId AND Status != Inactive`
4. **404 not 403** for cross-user access — don't confirm resource exists (anti-enumeration)
5. **PUT/DELETE log at Warning** for rejected cross-user attempts — monitoring signal

**Tests:**

- Unit: [AuthorizationTests.cs](../tests/todostodo.api.test/AuthorizationTests.cs) — ownership, cross-user 404, DB unchanged
- Integration: [AuthFlowIntegrationTests.cs](../tests/todostodo.api.test/Integration/AuthFlowIntegrationTests.cs) — full pipeline, 401 without auth

**Ezra bridge:** "Member imaging data isolation is the same principle — every query scoped to authenticated identity, fail closed, tested at unit and integration layers."

---

### §4 — Key tradeoffs (~1.5 min)


| Decision     | Chosen                     | Alternative    | When I'd switch                                   |
| ------------ | -------------------------- | -------------- | ------------------------------------------------- |
| Auth         | Cookie + Identity          | JWT            | Native mobile clients, third-party API consumers  |
| DB           | SQLite in-memory           | EF InMemory    | Postgres when durability or multi-instance needed |
| Delete       | Soft (`Inactive`)          | Hard delete    | Already chose soft for future trash/restore       |
| API          | REST + OpenAPI             | GraphQL        | Never for this CRUD scope                         |
| Status       | Forward-only               | Loop to Active | N/A — intentional UX choice                       |
| Multi-window | Pop-out + BroadcastChannel | Resize browser | N/A — pop-out is more reliable                    |


**Reference:** [DESIGN.md §3](DESIGN.md)

---

### §5 — UX and product choices (~1 min)

- **Composer row** at top — Enter to add, no extra clicks
- **Status cycle** — Active → InProgress → Completed → Archived, stops (no accidental undo)
- **Sort** — InProgress floats top, Archived sinks bottom (client-side for MVP)
- **Pop-out window** (~440×720) — side-panel notebook UX for multitasking
- **Cross-tab sync** — BroadcastChannel + focus refetch; last-write-wins (documented limitation)
- **Settings** — per-user font via dedicated `/api/settings`; desktop popover stays open during font preview

**Reference:** [useEntries.ts](../src/todostodo.web/src/hooks/useEntries.ts), [crossTabSync.ts](../src/todostodo.web/src/lib/crossTabSync.ts)

---

### §6 — Ops and production readiness (~1 min)

- **CI** — [.github/workflows/ci.yml](../.github/workflows/ci.yml): API tests, web tests + Vite build, Docker build (parallel)
- **Docker** — multi-stage: Node build → .NET publish → single port :8080
- **Logging** — Serilog + structured request logging
- **Errors** — [GlobalExceptionHandler.cs](../src/todostodo.api/Middleware/GlobalExceptionHandler.cs): 500 ProblemDetails, no stack trace to client

---

### §7 — Honest gaps (~1 min) — say these before they ask

From [DESIGN.md §5, §8](DESIGN.md):

- In-memory DB — no durability; `EnsureCreated()` not migrations
- No pagination, virtualization, or performance testing
- No distributed session store (Redis) for horizontal scale
- CSRF: SameSite + same-origin; would add antiforgery in production
- Integration tests use Bearer tokens; SPA uses cookies — need cookie-based E2E
- Known edge case: Completed → Archived in pop-out windows

**Framing:** "I know what production at Ezra scale adds — migrations, audit logging, encryption at rest, observability, compliance review. Here's the foundation and the roadmap."

---

### §8 — What I'd do next at Ezra scale (~30 sec)

1. Postgres + EF migrations + indexes on `(UserId, Status)`
2. Paginated `GET /api/Entry`
3. Redis session store if multi-instance
4. Playwright E2E with real cookies
5. Health checks + metrics

---

## Rehearsal checklist

- [ ] Deliver 2-min narrative without reading (use bullets above as prompts)
- [ ] Walk through §1–§8 in order in ~10 minutes
- [ ] Be ready to go deep on §3 (security) if Kristina asks
- [ ] Be ready to go deep on §4 + §7 (tradeoffs + scale) if Michael asks
- [ ] Have repo open to EntryController and useEntries during walkthrough