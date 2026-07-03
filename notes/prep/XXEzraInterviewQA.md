# Ezra Interview Prep — Q&A

**Convention:** Questions asked during interview prep get answered here (or merged into another prep doc when they're a better fit). Newest entries at the top.

**Related docs:** [EzraPanelInterviewOutline.md](EzraPanelInterviewOutline.md) (cheat sheet) · [EzraRecruiterDetails.md](EzraRecruiterDetails.md) (role JD)

---

## 2026-07-01 — What is multi-tenant data isolation?

**Q:** What is multi-tenant data isolation?

**A:** **Multi-tenant** = one application serves **many customers (tenants)** on shared infrastructure (same app, same database). **Data isolation** = each tenant can **only access their own data** — never another tenant's.

In your take-home, each **user is a tenant**. All users share one API and one `Entries` table, but User A must never see or change User B's todos.

### How you implement it

1. **Authenticate** — know who is making the request (Identity cookie → `ClaimTypes.NameIdentifier`)
2. **Scope every query** — always filter by that user's id:

```csharp
.Where(e => e.UserId == userId && e.Status != EntryStatus.Inactive)
```

3. **Stamp on create** — `UserId` from the claim, never from the request body
4. **404 for cross-user access** — don't leak whether another user's row exists
5. **Test it** — `AuthorizationTests` seeds two users, verifies cross-user GET/PUT/DELETE returns 404 and DB unchanged

### The bug you fixed

Early build returned **all users' entries** to any logged-in user — **isolation failure**. In healthtech, the same class of bug with member imaging data would be a serious incident.

### Multi-tenant vs single-tenant

| | Single-tenant | Multi-tenant (your app) |
|--|---------------|-------------------------|
| **Deployment** | One customer, their own stack | Many users, shared app + DB |
| **Isolation** | Physical separation | Logical — every query filtered by tenant id |
| **Ezra scale** | Unlikely per member | Members share platform; PHI scoped per member/account |

### Ezra bridge

Ezra members share imaging/scheduling platforms. **Member A's scan results must never appear in Member B's account** — same pattern as your `UserId` filter, with higher stakes (HIPAA, audit trails, encryption).

### Interview one-liner

> "Multi-tenant isolation means many users on one system but each only sees their own data. I enforce that by scoping every database query to the authenticated user's id from the identity claim, never from the client, with tests that prove cross-user access fails."

**Related:** [EntryController.cs](../../src/todostodo.api/Controllers/EntryController.cs) · [AuthorizationTests.cs](../../tests/todostodo.api.test/AuthorizationTests.cs) · [DESIGN.md §6](../DESIGN.md)

---

**Q:** Batch of interview prep questions from code tour / narrative.

---

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

---

### OpenAPI — short interview answer

**What:** A **machine-readable description of your REST API** — endpoints, request/response shapes, status codes. Your API exposes it as **Swagger** at `/swagger`.

**Why you use it:** Run `pnpm generate-api` → TypeScript client in `src/api/generated/`. Frontend and backend share one contract; renames or type changes break the build instead of failing silently at runtime.

**One-liner:** "OpenAPI is the API contract. Swagger exposes it; codegen turns it into typed fetch calls so the frontend can't drift from the backend."

---

### TanStack Query — short interview answer

**What:** A **server-state library** for React — fetching, caching, loading/error states, refetching, mutations.

**Why you use it:** All API data (auth, entries, settings) lives in a query cache. Mutations use `onMutate` / rollback / `invalidateQueries` for optimistic updates. Auth uses `staleTime: Infinity` until login/logout changes it.

**One-liner:** "TanStack Query owns anything that came from the API — cache, loading states, optimistic updates, and refetch after mutations."

---

### TanStack Router — short interview answer

**What:** **File-based, type-safe routing** for React. Routes live in `src/routes/`; `routeTree.gen` is generated.

**Why you use it:** `/` runs `beforeLoad` — checks auth via `ensureQueryData(authQueryOptions)` and redirects to `/login` if null. Typed navigation, route loaders tied to QueryClient.

**One-liner:** "TanStack Router handles URLs and guards — the home route won't render until auth is confirmed in the query cache."

---

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

---

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

---

### Tests you have vs could add

**Have (65 API + 17 frontend):**

| Area | Files |
|------|--------|
| Entry CRUD, soft delete, timestamps | EntryControllerTests |
| Cross-user 404, UserId from claim | AuthorizationTests |
| DTO validation | ValidationTests |
| GlobalExceptionHandler 500 + ProblemDetails | GlobalExceptionHandlerTests |
| Settings + font allowlist | SettingsControllerTests |
| Auth pipeline (register/login/401) | AuthFlowIntegrationTests |
| Validation 400, CORS | EntryEndpointIntegrationTests |
| Optimistic update rollback | useEntries.test.tsx |
| Composer, authErrors | ComposerRow.test.tsx, authErrors.test.ts |

**Gaps / high-value adds:**

| Missing | Why it matters |
|---------|----------------|
| **Playwright E2E with cookies** | Integration tests use **Bearer tokens**; real SPA uses **cookies** — different code path |
| **Cookie login integration test** | `POST /login?useCookies=true` then `GET /api/Entry` with cookie jar |
| **SettingsController integration** | Only unit tests today |
| **Logout clears session** | Cookie-based logout → subsequent 401 |
| **Frontend route guard** | `/` redirects when logged out |
| **CrossTabSync / BroadcastChannel** | Hard to test; optional |
| **Kind update** | Skipped intentionally (`Update_PersistsNewKind`) until UI exists |
| **GlobalExceptionHandler integration** | Unit tests only |
| **Load / pagination** | Not built yet |

**Interview line:** "Strong unit + integration pyramid for auth and ownership; biggest gap is cookie-based E2E matching how the SPA actually ships."

---

### Redis session store if multi-instance (with example)

**Scenario:** 3 API pods behind a load balancer. User logs in on pod A.

**Without Redis:**
```
Login → pod A stores session in A's RAM
Next request → pod B → "who?" → 401
```

**With Redis:**
```
Login → pod A writes session:{cookieId} → Redis
Next request → pod B reads session:{cookieId} from Redis → OK
```

**Concrete example:**
1. User POST `/login?useCookies=true` → response `Set-Cookie: .AspNetCore.Identity.Application=abc123`
2. Redis key: `session:abc123` → `{ userId: "cbd60e20-...", expires: ... }`
3. User GET `/api/Entry` with cookie → any pod looks up `abc123` in Redis → loads user → returns entries

**ASP.NET:** `AddStackExchangeRedisCache` + configure Identity/DataProtection to use distributed store.

**Related:** [EzraInterviewQA.md — 3 API instances](EzraInterviewQA.md)

---

### Playwright E2E with real cookies (with example)

**Problem:** [AuthFlowIntegrationTests.cs](../tests/todostodo.api.test/Integration/AuthFlowIntegrationTests.cs) uses `Authorization: Bearer {token}`. Your SPA uses `credentials: 'include'` and Identity cookies — **not exercised end-to-end in CI**.

**Playwright test sketch:**
```typescript
test('register, add todo, logout', async ({ page }) => {
  await page.goto('/login')
  await page.fill('[name=email]', 'e2e@test.com')
  await page.fill('[name=password]', 'TestPass1!')
  await page.click('text=Create account')
  // ... login flow ...
  await page.fill('[placeholder=...]', 'Buy milk')
  await page.keyboard.press('Enter')
  await expect(page.getByText('Buy milk')).toBeVisible()
  await page.click('text=Sign out')
  await expect(page).toHaveURL('/login')
})
```

Playwright's browser **stores cookies automatically** — same as a real user. Catches proxy misconfig, wrong `useCookies`, broken logout, CORS credential issues.

**Interview line:** "Integration tests prove the auth middleware; Playwright would prove the full browser cookie path the member actually uses."

---

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

**Q:** Is this representative of a diagram? (`EzraInterviewNarrative.md` §2 — Browser → API → SQLite) How would one describe it in words?

**A:** Yes — it's a **simplified layer / stack diagram** (sometimes called a **logical architecture** or **three-tier** sketch). It's not a full system diagram (no load balancer, Docker, CI, OpenAPI codegen, React Query, etc.), but it's the right **60-second backbone** for a walkthrough.

### What the ASCII shows

```
Browser (React + Vite)
    ↓ credentials: 'include' (cookies)
ASP.NET Core API (Identity, controllers)
    ↓ EF Core
SQLite in-memory
```

| Layer | Role |
|-------|------|
| **Top** | SPA in the browser — UI, TanStack Query cache |
| **Arrow** | HTTP + JSON; auth cookie sent on every request |
| **Middle** | API — auth, business logic, REST endpoints |
| **Arrow** | EF Core ORM talks to the database |
| **Bottom** | SQLite in-memory (demo persistence) |

### How to say it out loud (~30–45 sec)

> "It's a **decoupled SPA plus API**. The browser runs a React app built with Vite. All API calls use **cookie-based auth** — the client sends `credentials: include` so the Identity session cookie goes with every request.
>
> The **ASP.NET Core API** handles authentication, REST controllers for entries and settings, logging, and error handling. It doesn't embed business rules in the frontend — the API is the source of truth.
>
> Data access goes through **EF Core** down to **SQLite in-memory** for this demo. In production I'd swap that for Postgres or SQL Server with migrations, but the shape stays the same: browser → API → database."

### What to add if they want more detail (don't cram into the first pass)

- **Contract:** OpenAPI / Swagger → generated TypeScript client
- **Frontend state:** TanStack Query wraps the generated client
- **Dev vs prod:** two processes locally (Vite proxy); one Docker container in prod (API serves the built SPA)
- **Security:** Identity cookies, `[Authorize]` on controllers, UserId from claims

### If they ask you to "draw it"

Draw three boxes top to bottom: **React SPA** → **ASP.NET API** → **Database**. Label the arrows "HTTPS + JSON + session cookie" and "EF Core". That's enough for a whiteboard.

**Related:** [EzraInterviewNarrative.md §2](EzraInterviewNarrative.md)

---

**Q:** What are HL7 and DICOM?

### DICOM

**DICOM** (Digital Imaging and Communications in Medicine) is the **standard format and protocol for medical imaging** — MRI, CT, X-ray, ultrasound, etc.

It covers:
- **Image files** — how scan slices are stored (metadata + pixel data)
- **Networking** — how imaging devices and PACS (Picture Archiving and Communication Systems) send/receive studies
- **Metadata** — patient id, study date, modality, series, orientation, etc. embedded in the file

**Ezra relevance:** Members get **full-body MRIs** at partner imaging centers. Those scans are almost certainly DICOM. Software that handles images (storage, routing, radiologist viewers, AI analysis on prostate MRI mentioned in their materials) typically speaks DICOM.

**Your honest position:** No SWE experience with DICOM yet. You understand it's the imaging equivalent of "PDF for medical scans — but with strict metadata and clinical workflow rules."

---

### HL7

**HL7** (Health Level Seven) is a family of **standards for exchanging clinical and administrative data** between healthcare systems — not images, but *information about patients and care*.

Common examples:
- Lab results sent from a lab system to a hospital EMR
- ADT messages (Admit, Discharge, Transfer) when a patient enters/leaves a facility
- Orders and results between systems

**FHIR** (Fast Healthcare Interoperability Resources) is the **modern HL7 standard** — REST/JSON APIs instead of older pipe-delimited messages (HL7 v2). When job posts say "HL7/FHIR," they mean integrating with EMRs, scheduling systems, and partner clinics via standardized healthcare APIs.

**Ezra relevance:** Recruiter JD mentions APIs integrating with **EMRs, imaging partners, and AI services**. Scheduling pod likely exchanges appointment/patient context with external systems — often HL7 v2 or FHIR in healthcare.

**Your honest position:** No direct HL7/FHIR implementation experience. Parallel: you've built **third-party API integrations** (Lighter webhooks, Carta APIs) and worked under **compliance constraints** (Zapier SOC2/GDPR). Clinical front-desk work gives you intuition for why standardized patient data exchange matters.

---

### Quick comparison

| | DICOM | HL7 / FHIR |
|--|-------|------------|
| **What** | Medical **images** and imaging workflow | Clinical **data** and messages |
| **Examples** | MRI slice, scan series, PACS | Lab result, appointment, patient demographics |
| **Ezra pod** | Reporter / Images (likely) | Scheduling / integrations (likely) |
| **Your experience** | None in software | None in software — integration + compliance transfer |

### Interview one-liner

> "DICOM is how medical imaging is stored and moved — relevant to Ezra's MRI pipeline. HL7 and FHIR are how healthcare systems exchange patient and clinical data — relevant to EMR and scheduling integrations. I haven't implemented either, but I've built regulated third-party integrations and I'm eager to learn the healthcare-specific standards on the job."

**Related:** [EzraPanelInterviewOutline.md § Role Expectations — nice-to-have](EzraPanelInterviewOutline.md) · [EzraRecruiterDetails.md](EzraRecruiterDetails.md)

---

**Q:** What do you mean by "3 API instances" and Redis session store?

### 3 API instances

**One API instance** = one running copy of your ASP.NET Core app (e.g. `dotnet run` on `:5162`, or one Docker container).

**3 API instances** = three identical copies behind a **load balancer** for capacity and reliability:

```
Browser → Load balancer → API instance 1
                       → API instance 2
                       → API instance 3
                              ↓
                         shared database
```

Each request goes to one instance. If one crashes, others keep serving. This is **horizontal scaling**.

### Why it breaks your cookie auth today

With cookie sessions, login creates **session state in that API process's memory**. The cookie is just an ID pointing to that state.

- Login hits **instance 1** → session lives in instance 1's RAM
- Next request hits **instance 2** → instance 2 has no record of that session → **401 Unauthorized**

User appears logged out even though the cookie is still valid. [DESIGN.md §5](DESIGN.md): cookie sessions work for a single instance; multiple instances need shared session storage.

### Redis session store

**Redis** = fast in-memory store shared by all API instances.

Instead of each server keeping sessions locally:

```
Instance 1 ──┐
Instance 2 ──┼──→ Redis (shared sessions)
Instance 3 ──┘
```

1. Login on instance 1 → session written to Redis (keyed by session id from cookie)
2. Next request on instance 2 → reads same session from Redis → still logged in

ASP.NET Core supports this via **distributed cache / session store** (Redis is common; SQL Server session state is an alternative).

### Alternatives (know the tradeoffs)

| Approach | Pros | Cons |
|----------|------|------|
| **Redis session store** | All instances share login state; proper fix | Extra infra to run and monitor |
| **Sticky sessions** | Load balancer always routes same user to same instance | Weaker — instance death logs users out; uneven load |
| **JWT (stateless)** | Scales easily, no shared store | Hard server-side logout (why you chose cookies) |

### Interview one-liner

> "Three API instances means three copies of the app behind a load balancer. My cookie sessions are in-memory per process, so a request landing on a different instance than login would 401. I'd add a distributed session store like Redis so all instances read the same session data."

**Related:** [EzraInterviewWhatIfs.md — multiple instances](EzraInterviewWhatIfs.md) · [DESIGN.md §5 Auth](DESIGN.md)

---

## 2026-07-01 — BroadcastChannel refetch vs ETags (for later)

**Q:** Explain the BroadcastChannel refetch and ETags for later.

### The problem (your pop-out window)

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

| | BroadcastChannel refetch | ETags |
|--|--------------------------|-------|
| Fixes stale cache across tabs | Yes | Yes (after refetch or on 409) |
| Detects simultaneous edits | No | Yes |
| Prevents silent overwrite | No | Yes |

**In .NET:** Often implemented with SQL Server `rowversion` / EF Core concurrency token on `Entry`, exposed as ETag or checked in the PUT handler.

**Interview arc:** "For MVP I used BroadcastChannel because the DB is already correct — I only needed caches to catch up. At scale, or if two clinicians edit the same record, I'd add ETags or a `RowVersion` concurrency token so conflicting writes return 409 instead of silently overwriting."

**Related:** [EzraInterviewWhatIfs.md — two tabs editing](EzraInterviewWhatIfs.md) · [DESIGN.md §8](DESIGN.md) (last-write-wins documented)

---

**Q:** What is virtualizing?

**A:** **List virtualization** (or “windowing”) means the UI only renders the rows that are **visible on screen** (plus a small buffer), not every item in the dataset.

**Without virtualization:** 10,000 todos → 10,000 DOM nodes → slow scroll, high memory, janky updates.

**With virtualization:** Still 10,000 items in memory/data, but maybe ~20 DOM nodes — one per visible row. As you scroll, rows are recycled: old ones unmount, new ones mount.

**In your take-home:** The todo list renders **all** entries from React Query. Fine for small lists; breaks at thousands of rows. Fix = pagination on the API **and** a virtualized list component on the frontend (e.g. `@tanstack/react-virtual`, `react-window`).

**Interview one-liner:** “Virtualization keeps DOM size constant regardless of list length — you only paint what’s in the viewport.”

**Related what-if:** [EzraInterviewWhatIfs.md — 10,000 todos](EzraInterviewWhatIfs.md)

---

**Q:** Moving forward, for questions I ask regarding interview prep, either add the answers where useful in the doc, or create a new file for Q&A.

**A:** Use this file (`EzraInterviewQA.md`) as the default for ad-hoc prep questions. If an answer belongs in a specific doc (e.g. a new STAR story → `EzraInterviewSTARStories.md`, a new what-if → `EzraInterviewWhatIfs.md`), put it there and add a short cross-link here so nothing gets lost.

---

## 2026-07-01 — Was EzraRecruiterDetails.md used when building the plan?

**Q:** It looks like `EzraRecruiterDetails.md` wasn't saved earlier — were you able to include it as reference when building the plan?

**A:** **Partially, but not from that file directly.** When the plan was first built, `EzraRecruiterDetails.md` was empty. Prep drew from `EzrajobPost.md`, `EzraInterviewTips.md`, your resume, README, and DESIGN.md instead.

The imaging-team specifics (EMR/imaging partner integrations, Azure/SQL Server, mentoring, DICOM/HL7) weren't in the prep until the file was saved. Overlap with the job post existed (.NET, full-stack, HIPAA nice-to-have), but not the imaging-team role description in full.

**Follow-up:** Role expectations from the recruiter JD (non-overlapping items only) were added to [EzraPanelInterviewOutline.md § Role Expectations](EzraPanelInterviewOutline.md).

---

## 2026-07-01 — API test run failed, then succeeded

**Q:** (System notification) First API test run failed.

**A:** First run failed due to sandbox timeout and NuGet restore error. Re-run with extended timeout: **65 passed, 1 skipped** (`Update_PersistsNewKind`). Documented in [EzraInterviewLiveDemo.md](EzraInterviewLiveDemo.md).

---

## 2026-07-01 — Dev servers stopped

**Q:** (System notifications) Vite and API servers were aborted.

**A:** Expected — they were only running for live verification during prep. Restart before interview:

```bash
dotnet run --project src/todostodo.api/todostodo.api.csproj
cd src/todostodo.web && pnpm dev
```

Or: `docker compose up --build` → http://localhost:8080

---

## 2026-07-01 — Panel and prep materials (initial request)

**Q:** Prepare for panel with Kristina Matiukhina (Team Lead) and Michael Hambor (Staff Engineer). Senior leveling. Include behavioral/technical Q&A with examples. Flexible at-a-glance outline.

**A:** Full prep package created under `notes/`:

| File | Purpose |
|------|---------|
| [EzraPanelInterviewOutline.md](EzraPanelInterviewOutline.md) | At-a-glance cheat sheet — **start here on interview day** |
| [EzraInterviewNarrative.md](EzraInterviewNarrative.md) | 2-min script + 10-min walkthrough |
| [EzraInterviewSTARStories.md](EzraInterviewSTARStories.md) | Resume STAR stories with full scripts |
| [EzraInterviewCodeTour.md](EzraInterviewCodeTour.md) | Key files to know cold |
| [EzraInterviewWhatIfs.md](EzraInterviewWhatIfs.md) | Technical what-if Q&A |
| [EzraInterviewQuestions.md](EzraInterviewQuestions.md) | Questions to ask Kristina & Michael |
| [EzraInterviewLiveDemo.md](EzraInterviewLiveDemo.md) | Test results + manual UI checklist |

**Panel read:**
- **Kristina** — delivery, testing, data quality, mentoring (QA → data eng → fullstack; HIPAA at Klick)
- **Michael** — architecture, prod debugging, tradeoff depth (Staff; ex-Klick data platform)

**Prior candidate fail mode:** weak on production incidents/support — lead with Zapier deletion story (Story A).
