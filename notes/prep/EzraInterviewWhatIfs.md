# Ezra Interview — What-If Q&A

**Practice out loud.** Each answer: direct answer → reasoning → what you'd do at scale (if applicable).

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

---

### "What if the API restarts in production?"

**This app:** All data gone — SQLite in-memory. Documented in README and DESIGN.md §5.

**Production path:** Postgres + EF migrations, connection pooling, graceful shutdown, health check endpoint, backup/restore runbook.

**Incident framing:** If users report missing data after deploy → first question: "Did the process restart?" Correlate with deploy timestamp and pod lifecycle.

---

### "What if you run 3 API instances behind a load balancer?"

Cookie sessions are in-memory per instance. User hits instance B with session from A → logged out or auth fails.

**Fix:** Distributed session store — Redis (usual for ASP.NET Core) or SQL Server session state. Sticky sessions are a weaker shortcut.

**Resume tie:** Worked with Redis at Zapier scale — same pattern.

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
