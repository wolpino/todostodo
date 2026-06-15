Clean. Here's the full picture of what's there, what was just fixed, and what to leave for later:

---

### What just changed — the one real gap

`queryClient.ts` was retrying **every** query failure once, including 401 and 404 which will produce the same result a second time. The fix:

1. `HttpError` class (`lib/httpError.ts`) — carries the HTTP status alongside the message
2. `useEntries` now throws `HttpError` instead of a plain `Error`, so the status is available to the retry logic
3. `queryClient.ts` retry function now skips retry for any 4xx and only retries on 5xx or no-response (network drop)

---

### What's fine as-is

**Mutations with no retry** — correct. Create/update/delete are not safe to retry blindly without idempotency keys. If a create request goes out and you don't know if it landed, retrying produces a duplicate. The current behavior (fail, roll back optimistic update, user sees the error) is the right default until idempotency is designed.

**Auth query with `retry: false`** — correct. If the auth check returns 401 the user isn't logged in; retrying it accomplishes nothing and just delays the redirect to login.

**No backend retry policy (Polly, EF resilience)** — not needed yet. The database is SQLite in-process; there's no network hop for EF to retry. When/if this moves to Postgres or SQL Server, `EnableRetryOnFailure()` belongs in the `AddDbContext` call. Nothing to do today.

`erasableSyntaxOnly` requires that all TypeScript-specific syntax compiles away without emitting any JavaScript — parameter properties don't qualify because they generate field assignment code. The fix is the standard JS class pattern: declare the field on the class body, assign it in the constructor body.