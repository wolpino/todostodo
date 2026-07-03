# Ezra Interview — 2-Min Narrative + 10-Min Walkthrough

**Practice out loud.** Target: 2 minutes for opener, ~10 minutes for walkthrough (they may go deeper on any section).

---

## 2-Minute Core Narrative (script)

> I built a MVP of a minimal todo app called Todos To Do. I interpreted the assignment's  requirement of 'production minded' as needing real auth, multi-tenant data isolation,  tests, Docker, CI, and structured logging.
>
> Architecturally: decoupled React SPA plus [ASP.NET](http://ASP.NET) Core API, cookie sessions via Identity for real server-side logout, OpenAPI codegen for type safety, TanStack Query for optimistic UX, SQLite in-memory for the demo with a documented path to Postgres and pagination.
>
> Ease of use was top of mind for me, minimal clicks and an intuitive workflow would be necessary. While I felt simplicity was important, I wanted to pair it with visual customization to make it more appealing to more people. (Honestly, this thought came from an episode of Gilmore Girls when Rory’s grandfather helps her on a school project and they make customizable first aid kits for kids lockers. )
>
> My personal goal of this project was to experiement with how best to add generative AI to my workflow. I used Copilot in VSC and Cursor through the project and will expand on usage later.

---

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
### RFC 7807 ProblemDetails

**What it is:** A standard **JSON shape for HTTP error responses** so clients get predictable fields instead of random error HTML or raw stack traces.

**RFC 7807** defines fields like:
- `type` — URI identifying the error category
- `title` — short human-readable summary
- `status` — HTTP status code
- `detail` — optional longer explanation (your handler omits this on 500 — intentional)
- `instance` — optional URI for this specific occurrence

**In your app:** [GlobalExceptionHandler.cs](../src/todostodo.api/Middleware/GlobalExceptionHandler.cs) returns:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An unexpected error occurred.",
  "status": 500
}
```

**Interview line:** "Unhandled exceptions go through one handler. We log the full exception server-side but return RFC 7807 ProblemDetails to the client — no stack trace leakage."




- CORS whitelist with credentials

**Frontend** — [main.tsx](../src/todostodo.web/src/main.tsx):

- OpenAPI → `pnpm generate-api` → typed client in `src/api/generated/`
- TanStack Query for all server state (auth, entries, settings)
TanStack Query — short interview answer

What: A server-state library for React — fetching, caching, loading/error states, refetching, mutations.

Why you use it: All API data (auth, entries, settings) lives in a query cache. Mutations use onMutate / rollback / invalidateQueries for optimistic updates. Auth uses staleTime: Infinity until login/logout changes it.

One-liner: "TanStack Query owns anything that came from the API — cache, loading states, optimistic updates, and refetch after mutations."

- TanStack Router with auth guard on `/`

TanStack Router — short interview answer

What: File-based, type-safe routing for React. Routes live in src/routes/; routeTree.gen is generated.

Why you use it: / runs beforeLoad — checks auth via ensureQueryData(authQueryOptions) and redirects to /login if null. Typed navigation, route loaders tied to QueryClient.

One-liner: "TanStack Router handles URLs and guards — the home route won't render until auth is confirmed in the query cache."

**Dev vs prod:** Dev = split processes (API :5162, Vite :5173 with proxy). Prod = single Docker container, API serves SPA on :8080.



**A:** Yes — it's a **simplified layer / stack diagram** (sometimes called a **logical architecture** or **three-tier** sketch). It's not a full system diagram (no load balancer, Docker, CI, OpenAPI codegen, React Query, etc.), but it's the right **60-second backbone** for a walkthrough.

### What the ASCII shows

```
Browser (React + Vite)
    ↓ credentials: 'include' (cookies)
ASP.NET Core API (Identity, controllers)
    ↓ EF Core
SQLite in-memory
```


| Layer      | Role                                           |
| ---------- | ---------------------------------------------- |
| **Top**    | SPA in the browser — UI, TanStack Query cache  |
| **Arrow**  | HTTP + JSON; auth cookie sent on every request |
| **Middle** | API — auth, business logic, REST endpoints     |
| **Arrow**  | EF Core ORM talks to the database              |
| **Bottom** | SQLite in-memory (demo persistence)            |


### How to say it out loud (~30–45 sec)

> "It's a **decoupled SPA plus API**. The browser runs a React app built with Vite. All API calls use **cookie-based auth** — the client sends `credentials: include` so the Identity session cookie goes with every request.
>
> The **ASP.NET Core API** handles authentication, REST controllers for entries and settings, logging, and error handling. It doesn't embed business rules in the frontend — the API is the source of truth.
>
> Data access goes through **EF Core** down to **SQLite in-memory** for this demo. In production I'd swap that for Postgres or SQL Server with migrations, but the shape stays the same: browser → API → database."

### What to add if they want more detail (don't cram into the first pass)

- **Contract:** OpenAPI / Swagger → generated TypeScript client
### OpenAPI — short interview answer

**What:** A **machine-readable description of your REST API** — endpoints, request/response shapes, status codes. Your API exposes it as **Swagger** at `/swagger`.

**Why you use it:** Run `pnpm generate-api` → TypeScript client in `src/api/generated/`. Frontend and backend share one contract; renames or type changes break the build instead of failing silently at runtime.

**One-liner:** "OpenAPI is the API contract. Swagger exposes it; codegen turns it into typed fetch calls so the frontend can't drift from the backend."



- **Frontend state:** TanStack Query wraps the generated client

What: A server-state library for React — fetching, caching, loading/error states, refetching, mutations.

Why you use it: All API data (auth, entries, settings) lives in a query cache. Mutations use onMutate / rollback / invalidateQueries for optimistic updates. Auth uses staleTime: Infinity until login/logout changes it.

One-liner: "TanStack Query owns anything that came from the API — cache, loading states, optimistic updates, and refetch after mutations."

- **Dev vs prod:** two processes locally (Vite proxy); one Docker container in prod (API serves the built SPA)
- **Security:** Identity cookies, `[Authorize]` on controllers, UserId from claims

### If they ask you to "draw it"

Draw three boxes top to bottom: **React SPA** → **ASP.NET API** → **Database**. Label the arrows "HTTPS + JSON + session cookie" and "EF Core". That's enough for a whiteboard.

**Related:** [EzraInterviewNarrative.md §2](EzraInterviewNarrative.md)

---

### §3 — Security and data isolation (~2 min) ★ Know this cold

**The bug I fixed:** Early build returned all users' entries. Made UserId scoping non-negotiable.

**Patterns in** [EntryController.cs](../src/todostodo.api/Controllers/EntryController.cs):

1. **Class-level `[Authorize]`** — secure default; new endpoints can't ship without auth
2. **UserId from claim only** — `ClaimTypes.NameIdentifier`, never from request body

### ClaimTypes.NameIdentifier vs request body

**NameIdentifier:** A standard claim on the authenticated user's identity — in ASP.NET Identity it's the **user's primary key (GUID string)** set at login from the auth cookie/token.

```csharp
private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
// Create stamps: UserId = userId  (from claim)
```

**Never from request body:** `CreateEntryRequest` has Title, Status, Kind — **no UserId field**. If you allowed `UserId` in JSON, an attacker could POST `{ "title": "x", "userId": "someone-elses-id" }` and create or later reassign data.

**Rule:** Identity comes from **who authenticated** (claims), not from **what the client sends**.

**Example attack prevented:**
- Attacker logged in as Alice (claim = Alice's id)
- Malicious body says `userId: Bob`
- Correct code ignores body → entry owned by Alice only
- Wrong code → horizontal privilege escalation

3. **Every query scoped:** `WHERE UserId == userId AND Status != Inactive`
4. **404 not 403** for cross-user access — don't confirm resource exists (anti-enumeration)
5. **PUT/DELETE log at Warning** for rejected cross-user attempts — monitoring signal

### PUT/DELETE log at Warning — monitoring signal

**Code:** Cross-user or unknown-id **mutations** log at **Warning**; stale GET by id logs at **Debug**.

```csharp
logger.LogWarning("Update rejected — entry {EntryId} not found or not owned by user {UserId}", id, userId);
```

**Why Warning for PUT/DELETE but Debug for GET:**
- **GET** missing id — often innocent (stale UI, race after delete)
- **PUT/DELETE** to wrong id — more suspicious (probing sequential ids, tampering)

**Monitoring signal:** In production you'd alert on **spikes in Warning logs** with "not owned by user" — possible enumeration attack or bug. Structured logs (`EntryId`, `UserId`) make Datadog/Sentry queries easy.

**Interview line:** "404 avoids leaking existence; Warning-level logs on rejected mutations give ops something to alert on without noise from normal GET misses."


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
Each browser tab/window has its **own JavaScript memory**. React Query caches entries in that memory. When you open the pop-out:
- **Parent tab** and **pop-out** both share the **same auth cookie** and hit the **same API** — the database is consistent.
- But each window has a **separate React Query cache**. Edit in pop-out → parent still shows stale data until something triggers a refetch.

### What you built: BroadcastChannel + focus refetch

**Layer 1 — BroadcastChannel** ([crossTabSync.ts](../src/todostodo.web/src/lib/crossTabSync.ts)):

1. After a successful mutation, `useEntries` calls `broadcastSync({ type: 'entries-changed' })`.
2. That opens a `BroadcastChannel('todostodo-sync')`, posts a tiny message, closes.
3. Every other tab/window on the **same origin** listening on that channel receives the message.
4. The listener calls `queryClient.invalidateQueries(['entries'])` → React Query marks cache stale → active hooks **refetch from the API**.

So it's not pushing the new data directly — it's saying **"hey, something changed — go ask the server again."**

**Layer 2 — `window.focus` refetch:** When you click back into a tab, refetch entries + settings. Safety net if a broadcast was missed or the browser doesn't support BroadcastChannel.

**Mounted once** in [CrossTabSync.tsx](../src/todostodo.web/src/components/layout/CrossTabSync.tsx) at the app root.

### Limitation (be honest in interview)

**Last write wins.** If both tabs edit the **same todo at the same time**, whichever PUT hits the server last wins. BroadcastChannel only triggers refetch *after* a write — it doesn't merge concurrent edits or warn the user.

### "For later": ETags (optimistic concurrency)

**ETag** = a version stamp on a resource. Common pattern:

1. **GET** `/api/Entry/5` returns the entry **and** header `ETag: "abc123"` (often a hash of the row or a `RowVersion` column).
2. **PUT** includes `If-Match: "abc123"` — "only apply this update if the server still has this version."
3. If someone else updated first, server returns **409 Conflict** with the current version.
4. Client can: show "someone else changed this," merge, or reload.

**Why it's better than BroadcastChannel alone:**


|                               | BroadcastChannel refetch | ETags                         |
| ----------------------------- | ------------------------ | ----------------------------- |
| Fixes stale cache across tabs | Yes                      | Yes (after refetch or on 409) |
| Detects simultaneous edits    | No                       | Yes                           |
| Prevents silent overwrite     | No                       | Yes                           |


**In .NET:** Often implemented with SQL Server `rowversion` / EF Core concurrency token on `Entry`, exposed as ETag or checked in the PUT handler.

**Interview arc:** "For MVP I used BroadcastChannel because the DB is already correct — I only needed caches to catch up. At scale, or if two clinicians edit the same record, I'd add ETags or a `RowVersion` concurrency token so conflicting writes return 409 instead of silently overwriting."

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

### Health checks + metrics (with examples)

**Not in your take-home** — listed as production next steps.

**Health checks:** Endpoints the orchestrator (Kubernetes, Azure App Service) polls.

| Endpoint | Checks | Example response |
|----------|--------|------------------|
| `GET /health/live` | Process up | 200 OK |
| `GET /health/ready` | Can serve traffic? DB reachable? | 200 or 503 |

**Example:** After deploy, K8s calls `/health/ready` every 10s. SQLite connection dead → 503 → pod removed from load balancer until fixed.

**Metrics:** Numbers over time for dashboards/alerts.

| Metric | Example use |
|--------|-------------|
| `http_requests_total{status="500"}` | Alert if 500 rate spikes |
| `http_request_duration_seconds` | p95 latency regression |
| `auth_login_failures_total` | Brute-force signal |
| Custom: `entry_create_total` | Business activity |

**Your app today:** Serilog **logs** requests (text/structured) — good for debugging, not the same as metrics. Production = Serilog → aggregator **plus** Prometheus/Application Insights counters.

**Interview line:** "I have structured logging and a global exception handler; health checks and metrics would be the next ops layer for orchestration and alerting at Ezra scale."

---

### 9 - AI usage

> Around the time I remembered about solutions and projects it became clear what had been generated so far was messy and wrong.  
> , . And while I was rememebering more about .NET it didn't feel famliar enough to assess large code changes. I 
>
> I hadn't touched C# in five years — mostly Python at Zapier — so I **reset once** when AI-generated scaffolding got ahead of my ability to review it, then rebuilt with small, reviewable changes. That mirrors how I'd ramp on Ezra's .NET stack: learn fast, but never ship what I can't explain.

It had been about five years since I've written anything in C# and I knew I'd need some review, but unsure how much so I dove in with a plan and let the coding agent do it's thing. 

## Rehearsal checklist

- [ ] Deliver 2-min narrative without reading (use bullets above as prompts)
- [ ] Walk through §1–§8 in order in ~10 minutes
- [ ] Be ready to go deep on §3 (security) if Kristina asks
- [ ] Be ready to go deep on §4 + §7 (tradeoffs + scale) if Michael asks
- [ ] Have repo open to EntryController and useEntries during walkthrough