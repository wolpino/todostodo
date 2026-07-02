# Ezra Interview — Live Demo Verification

**Verified:** 2026-07-01  
**Purpose:** Confirm app works before panel interview; manual UI checklist for day-of.

---

## Automated verification results

| Check | Result | Notes |
|-------|--------|-------|
| API tests (`dotnet test`) | **65 passed**, 1 skipped | `Update_PersistsNewKind` skipped (MVP) |
| Frontend tests (`pnpm test:ci`) | **17 passed** | 3 files, Vitest |
| Docker build | **Not run** | Docker daemon not running on prep machine — run locally before interview |
| API cookie auth flow | **Pass** | Register → login `?useCookies=true` → CRUD → logout |
| Login page (UI) | **Pass** | http://localhost:5173/login loads with Sign in / Sign up |

---

## API flow verified (curl)

```
Register 200 → Login?useCookies=true 200 → GET /api/Entry 200 ([])
→ POST /api/Entry 201 → PUT status InProgress 204 → GET shows InProgress
→ GET /api/Settings 200 → PUT font "patrick-hand" 204
→ DELETE (soft) 204 → GET excludes deleted entry
→ POST /logout 200 → GET /api/Entry 401
```

**Note:** Login must use `?useCookies=true` for cookie sessions (same as SPA `useAuth.ts`).

**Settings font values:** `comic-shanns`, `courier-prime`, `patrick-hand`, `caveat` (legacy)

---

## Manual UI walkthrough (do this before interview)

Run from repo root:

```bash
# Option A — Docker (recommended for demo)
docker compose up --build
# Open http://localhost:8080

# Option B — Dev mode (two terminals)
dotnet run --project src/todostodo.api/todostodo.api.csproj
cd src/todostodo.web && pnpm dev
# Open http://localhost:5173
```

### Checklist

- [ ] **Register** — create account; lands on todo list (register then auto-login in UI)
- [ ] **Composer** — type todo, press Enter; appears in list
- [ ] **Status cycle** — click status bullet: Active → InProgress → Completed → Archived
- [ ] **Inline edit** — click todo text, edit, click away; saves
- [ ] **Delete** — X button removes row
- [ ] **Settings** — gear icon → change font; list updates (popover stays open on desktop — intentional)
- [ ] **Pop-out** — open small window button; add todo in pop-out; refetches in parent tab
- [ ] **Logout** — settings → sign out; redirects to login
- [ ] **Re-login** — todos persist (same session DB until API restart)

### Demo talking points while clicking

1. **Composer at top** — minimal clicks design choice
2. **Status sort** — InProgress floats top without filtered views
3. **Pop-out + BroadcastChannel** — side-panel UX; last-write-wins documented
4. **Auth** — cookie session; logout is server-side

---

## Files to have open during walkthrough

| File | Why |
|------|-----|
| [EntryController.cs](../src/todostodo.api/Controllers/EntryController.cs) | UserId scoping, 404 pattern |
| [useEntries.ts](../src/todostodo.web/src/hooks/useEntries.ts) | Optimistic updates |
| [AuthorizationTests.cs](../tests/todostodo.api.test/AuthorizationTests.cs) | Cross-user tests |
| [DESIGN.md](DESIGN.md) | Tradeoffs §3, gaps §8 |

---

## If something breaks during interview

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| Empty list after "login" | In-memory DB reset (API restarted) | Expected — explain durability gap |
| 401 on API calls | Cookie not sent / wrong origin | Same origin or Vite proxy; `credentials: 'include'` |
| Pop-out out of sync | Last-write-wins | Refocus tab or explain BroadcastChannel |
| Status stuck | Known Completed→Archived edge case | Acknowledge DESIGN.md §8 |

---

## Day-before commands

```bash
cd /Users/ari/codes/todostodo
dotnet test tests/todostodo.api.test/todostodo.api.test.csproj
cd src/todostodo.web && pnpm test:ci
docker compose up --build   # if Docker available
```
