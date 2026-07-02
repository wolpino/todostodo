# Ezra Interview — Code Tour Notes

**Files to have open during the interview.** Know location + one sentence per file.

---

## EntryController — auth and ownership

**File:** [src/todostodo.api/Controllers/EntryController.cs](../src/todostodo.api/Controllers/EntryController.cs)

### Key points to explain

1. **`[Authorize]` on the class (line 17)** — secure default is opt-out, not opt-in. New endpoints inherit auth automatically.

2. **`CurrentUserId` from claim (line 21)** — never trust UserId from request body.

3. **GET list (lines 31–33)** — filters `UserId == userId AND Status != Inactive`. Soft-deleted rows excluded.

4. **GET by id (lines 47–48)** — single query with `Id AND UserId`. Returns **404** for missing OR other user's entry.

5. **Create (lines 71–76)** — stamps `UserId = userId` from claim only.

6. **Update/Delete (lines 105–106, 154–155)** — same ownership query. Warning log on rejected cross-user attempts (possible probing).

7. **Delete = soft delete (line 165)** — sets `Status = Inactive`, row preserved for future trash/restore.

### If they ask "show me the security fix"

Open `Get()` and point to the `UserId` filter. Then open `AuthorizationTests.cs` and show cross-user test.

---

## useEntries — optimistic updates

**File:** [src/todostodo.web/src/hooks/useEntries.ts](../src/todostodo.web/src/hooks/useEntries.ts)

### Pattern (update mutation, lines 59–75)

```
onMutate  → cancel in-flight queries, snapshot cache, apply optimistic update
onError   → restore snapshot (rollback)
onSettled → invalidate queries (reconcile with server) + broadcastSync
```

### Why invalidate after success?

Server is source of truth. Sibling tabs (pop-out window) may have stale cache. `broadcastSync` tells other windows to refetch.

### Create vs update

- **Create:** not optimistic — waits for server, then prepends to cache (`onSuccess`)
- **Update/Delete:** optimistic with rollback

### Cross-tab sync

**File:** [src/todostodo.web/src/lib/crossTabSync.ts](../src/todostodo.web/src/lib/crossTabSync.ts)

- `BroadcastChannel('todostodo-sync')` — same-origin tabs/windows only
- `window.focus` also triggers refetch as fallback
- **Limitation:** last-write-wins if editing same todo in both tabs simultaneously

---

## GlobalExceptionHandler — error handling

**File:** [src/todostodo.api/Middleware/GlobalExceptionHandler.cs](../src/todostodo.api/Middleware/GlobalExceptionHandler.cs)

### Key points

1. Implements `IExceptionHandler` — centralized, not try/catch in every controller
2. **Logs at Error** with full exception (for debugging)
3. **Returns RFC 7807 ProblemDetails** — generic title, no stack trace to client
4. Status 500 always — client never sees internal details

### If they ask "what happens when the DB throws?"

Unhandled exception → GlobalExceptionHandler → 500 ProblemDetails + Error log. User sees "An unexpected error occurred." Ops sees full exception in logs.

---

## AuthorizationTests — regression prevention

**File:** [tests/todostodo.api.test/AuthorizationTests.cs](../tests/todostodo.api.test/AuthorizationTests.cs)

### Test layers

| Layer | File | What it proves |
|-------|------|----------------|
| Unit (ownership logic) | AuthorizationTests.cs | Cross-user GET/PUT/DELETE → 404; DB unchanged |
| Unit (controllers) | EntryControllerTests.cs | CRUD, soft delete, timestamps |
| Integration (pipeline) | AuthFlowIntegrationTests.cs | 401 without token; register/login/logout flow |
| Integration (HTTP) | EntryEndpointIntegrationTests.cs | Validation 400; CORS allow/deny |

### Tests to name if asked

- `Create_AssignsUserId_FromAuthenticatedUserClaim` — UserId never from body
- Cross-user update/delete tests — 404 + no DB mutation
- `CreateEntry_Returns401_WhenCalledWithoutToken` (integration)

---

## Program.cs — bootstrap (skim before interview)

**File:** [src/todostodo.api/Program.cs](../src/todostodo.api/Program.cs)

Worth mentioning if architecture comes up:

- SQLite `:memory:` with connection kept open (required for in-memory)
- `MapIdentityApi<User>()` + explicit `/logout` with `RequireAuthorization()`
- Middleware order: exception handler → auth → Serilog request logging
- Production: serves SPA from `wwwroot`, `MapFallbackToFile`

---

## Quick navigation cheat sheet

| Topic | Go to |
|-------|-------|
| Auth scoping | EntryController.cs lines 17, 31–33, 47–48 |
| Soft delete | EntryController.cs line 165 |
| Optimistic UX | useEntries.ts lines 59–75 |
| Cross-tab sync | crossTabSync.ts |
| Error handling | GlobalExceptionHandler.cs |
| Auth tests | AuthorizationTests.cs |
| Tradeoffs doc | notes/DESIGN.md §3 |
| Known gaps | notes/DESIGN.md §8 |
