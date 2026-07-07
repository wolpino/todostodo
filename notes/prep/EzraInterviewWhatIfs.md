# Ezra Interview — What-If Q&A

**Practice out loud.** Each answer: direct answer → reasoning → what you'd do at scale (if applicable).



## Incident Triage Script (any "what would you do if…")

1. **Scope** — who, when, % affected, deploy correlation?
2. **Mitigate** — rollback / feature flag / disable endpoint
3. **Diagnose** — logs (Serilog), errors (Sentry), recent changes
4. **Fix** — smallest safe change
5. **Communicate** — status + ETA to PM/support
6. **Prevent** — test, monitor, runbook, postmortem

**Example (their app):** "Todos disappeared after deploy" → in-memory DB reset on restart → persistent DB + health check + integration test.
---

## Scale and data

### "What if a user has 10,000 todos?"

**Today:** Unbounded `GET /api/Entry`, client-side sort — fine for demo, breaks at scale.

**Fix (in order):**

1. Server pagination — `?page=1&pageSize=50`, default sort by status priority then `ModifiedAt`
2. Index on `(UserId, Status, ModifiedAt)` in Postgres
3. Virtualize list in React — render only visible rows
4. Move sort/filter to API for mobile clients

**Senior note:** Don't add caching until metrics show read pressure — invalidation is harder than pagination.  

**List virtualization** (or “windowing”) means the UI only renders the rows that are **visible on screen** (plus a small buffer), not every item in the dataset.

**Without virtualization:** 10,000 todos → 10,000 DOM nodes → slow scroll, high memory, janky updates.

**With virtualization:** Still 10,000 items in memory/data, but maybe ~20 DOM nodes — one per visible row. As you scroll, rows are recycled: old ones unmount, new ones mount.

**In your take-home:** The todo list renders **all** entries from React Query. Fine for small lists; breaks at thousands of rows. Fix = pagination on the API **and** a virtualized list component on the frontend (e.g. `@tanstack/react-virtual`, `react-window`).

**Interview one-liner:** “Virtualization keeps DOM size constant regardless of list length — you only paint what’s in the viewport.”

---

### "What if the API restarts in production?"

**This app:** All data gone — SQLite in-memory. Documented in README and DESIGN.md §5.

**Production path:** Postgres + EF migrations, connection pooling, graceful shutdown, health check endpoint, backup/restore runbook.

**Incident framing:** If users report missing data after deploy → first question: "Did the process restart?" Correlate with deploy timestamp and pod lifecycle.

---

### "What if you run 3 API instances behind a load balancer?"

Cookie sessions are in-memory per instance. User hits instance B with session from A → logged out or auth fails.

**Fix:** Distributed session store — Redis (usual for ASP.NET Core) or SQL Server session state. Sticky sessions are a weaker shortcut.

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
```

```markdown
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
```

---

## Security

### "What if User A guesses User B's entry ID?"

Single query: `WHERE Id == id AND UserId == userId`. Returns **404** — doesn't confirm existence.

Mutation tests verify DB unchanged after rejected cross-user PUT/DELETE.

---

### "Why 404 instead of 403?"

403 confirms the resource exists but forbids access — information leakage for sequential IDs.

404 = "not found for you" whether missing or someone else's. Combined filter avoids separate "exists but forbidden" code path.

---

### "Why cookies instead of JWT?"

1. **Logout:** JWT can't revoke server-side without blocklist; cookies give real session invalidation
2. **Security:** HttpOnly cookies not accessible to JS — smaller XSS blast radius
3. **Fit:** Same-origin SPA; Identity API defaults to cookies

**When JWT:** Native mobile clients, third-party API consumers. Ezra likely has mixed model at scale.

---

### "What if someone steals a session cookie?"

**Today:** SameSite=Lax (CSRF mitigation), HttpOnly (XSS mitigation), HTTPS in prod (Secure flag).

**Healthtech additions:** Short session TTL, rotate on privilege change, audit log of sensitive reads, anomaly detection on IP/device. HIPAA often mandates session timeout.

**Gap:** No CSRF antiforgery tokens — acceptable for same-origin MVP; add for production cross-site forms.

---

## Frontend

### "What if two tabs edit the same todo simultaneously?"

**Current:** Last write wins. Optimistic updates in tab A unaware of tab B until refetch.

**Mitigation today:** BroadcastChannel + window focus refetch — best-effort, documented in DESIGN.md §8.

**At scale:** Optimistic concurrency — `ETag` or `RowVersion` on PUT; 409 Conflict returns server state; UI merges or prompts.

---

### "Why invalidate after optimistic success?"

Server is source of truth. Sibling tabs (pop-out window) may have stale cache. `onSettled` invalidates even after success to reconcile.

---

### "What if the assignment said Vue?"

Chose React to reduce review bandwidth while learning C#. Would ramp Vue quickly — same patterns: composition API, typed API client, query cache. Ezra uses Vue in production; I'd pair early and read existing components first.

---

## Process and judgment

### "Why did you wipe the repo and start over?"

AI generated C# faster than I could review — tests looked plausible but didn't match my mental model.

**Senior lesson:** AI is a junior pair programmer — great for boilerplate, dangerous for auth and data access. Restarted with manual scaffolding, tests first, small commits. Second pass was faster.

Not shameful — it's judgment.

---

### "Users report todos disappearing after deploy. Walk us through."

1. **Scope** — all users or subset? since when? deploy correlation?
2. **Mitigate** — rollback if widespread; post in incidents channel
3. **Diagnose** — Serilog 500 spike; auth cookie domain; soft-delete filter regression; **in-memory DB restart** (actual bug in this app)
4. **Fix** — hotfix or rollback; prod = persistent DB + integration test for data survival
5. **Prevent** — postmortem, regression test, monitor empty-list anomalies

---

### "How would you test this for production at Ezra?"

**Have:** Unit (controllers, auth, validation, exception handler), integration (auth pipeline, CORS, 400s), frontend hook tests (optimistic rollback).

**Add:** Playwright E2E with real cookies; OpenAPI contract tests; load test on paginated GET.

**Kristina angle:** Seed two users, verify zero cross-leakage; property-based tests on UserId scoping.

---

### "How would you add Note and Event entry types?"

`EntryKind` enum exists; UI only renders Todo. Kind update disabled in EntryController (skipped test).

**Next:** Enable kind on create in UI; `DueDate` column; validation differs by kind. Enable kind update with integration tests per kind.

---

## What you'd do differently (another week)

1. Postgres + EF migrations
2. Paginated GET
3. Playwright: register → create → status cycle → logout (cookies)
4. Fix Completed → Archived edge case in pop-out
5. Cookie-based integration test path (not just Bearer)

