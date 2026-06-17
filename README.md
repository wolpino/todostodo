# TodosToDo

A minimal todo web app: ASP.NET Core API + React frontend, cookie auth, and a fixed-width “notebook” UI. 

## Prerequisites

- [.NET SDK 10.0.300](https://dotnet.microsoft.com/download) (see `global.json`)
- [Node.js 22](https://nodejs.org/)
- [pnpm 11.6.0](https://pnpm.io/) (`corepack enable && corepack prepare pnpm@11.6.0 --activate`)

or

- [Docker](https://www.docker.com/) (optional, for one-command production run)

## Quick start (Docker)

- Install Docker

Run the following command from the repo root directory:

```bash
docker compose up --build
```

and open [http://localhost:8080](http://localhost:8080).  This builds the frontend, bundles it into the API, and serves everything on one port.

To stop and remove the container which will reset the in-memory database:

```bash
docker compose down
```

## Local development

1. Install:

[.NET SDK 10.0.300](https://dotnet.microsoft.com/download)  
[Node.js 22](https://nodejs.org/)  
[pnpm 11.6.0](https://pnpm.io/)

Dev mode runs the API and Vite dev server separately. The Vite proxy forwards `/api/*` and Identity routes to the API.

**Terminal 1 — API** ([http://localhost:5162](http://localhost:5162)):  
from the repo root directory:

```bash
dotnet run --project src/todostodo.api/todostodo.api.csproj
```

**Terminal 2 — Web** ([http://localhost:5173](http://localhost:5173)):

```bash
cd src/todostodo.web
pnpm install
pnpm dev
```

Open [http://localhost:5173](http://localhost:5173). Swagger is available at [http://localhost:5162/swagger](http://localhost:5162/swagger) in Development.

### Regenerate the API client

With the API running on `:5162`:

```bash
cd src/todostodo.web
pnpm generate-api
```

Output goes to `src/todostodo.web/src/api/generated/`.

## Tests

**API:**

```bash
dotnet test tests/todostodo.api.test/todostodo.api.test.csproj
```

**Frontend:**

```bash
cd src/todostodo.web
pnpm test:ci
```

CI (`.github/workflows/ci.yml`) runs API tests, web tests + Vite build, and `docker compose build` on pushes to `main`.

## Project layout

```
src/todostodo.api/     ASP.NET Core API (Identity auth, entries, settings)
src/todostodo.web/     React + Vite + Chakra UI
tests/todostodo.api.test/
notes/                 Product notes, design decisions, session logs
```

## Notes worth knowing

### Database

The app uses **SQLite in-memory** (`Data Source=:memory:`). Schema is created at startup via `EnsureCreated()` — there are no EF migrations.

- **Dev** (`dotnet run`) and **Docker** each have their own in-memory database.
- Restarting the API or running `docker compose down` wipes data.
- Data created in dev does not appear in Docker, and vice versa.

### Auth

ASP.NET Core Identity with **cookie-based** sessions (`credentials: 'include'` on the API client). Register and login are handled through the Identity API endpoints; the React app uses `/login` for the UI.

### Data model

Entries are stored in a **single flat table** with an `EntryKind` enum (`Todo`, `Note`, `Event`). Todo is the only kind implemented in the UI today; Note and Event are reserved for later. Kind is set on create and is not user-editable in the MVP.

### UI

- Fixed-width notebook shell (~420px), suitable for a narrow side window.
- Settings: font choice (persisted per user), sign out. Desktop uses a popover; mobile uses a bottom drawer.
- Composer row at the top: type and press Enter to add todos without extra clicks.

## Design decisions (summary)

- **Flat `Entry` + `EntryKind`**, not table-per-hierarchy — one list for MVP; less EF/API complexity than inheritance for now.
- **Cookie auth** (ASP.NET Identity), not JWT — I wanted real server-side logout; cookies came with Identity.
- **REST + OpenAPI → generated TS client** — typed and simple for CRUD; GraphQL felt like too much here.
- **TanStack Query** on top of the generated client — caching, loading state, optimistic updates.
- **SQLite in-memory** — assignment fit; schema via `EnsureCreated()`, no migrations yet.
- **Soft delete** (`Inactive`) — items leave the list without a trash/restore flow for now.
- **Dedicated settings endpoint** — room to grow (timezone, notifications) without overloading auth routes.
- **Decoupled SPA + API** — Vite proxy in dev; frontend bundled into the API image for Docker.

Full rationale and alternatives considered: [`notes/DESIGN.md`](notes/DESIGN.md).

## Assumptions

- Single-user, first-party web app — no sharing between accounts yet.
- In-memory DB is fine for demo; restart or `docker compose down` wipes data.
- Dev and Docker each have their own in-memory database (data does not carry over).
- One notebook-style list per user; only `Todo` kind is wired in the UI today (`Note` / `Event` reserved).
- Small list size — no pagination or virtualization yet.
- Narrow side-window use case; mobile works but desktop layout is the main target.

## Scaling & next steps

Not built for huge scale yet. Obvious next steps:

- **Data** — file-backed SQLite or Postgres, EF migrations, indexes on `UserId` / `Status`.
- **API** — pagination and filters on `GET /api/Entry` as lists grow.
- **Auth** — shared session store if the API runs on multiple instances.
- **Frontend** — virtualized list, fewer unnecessary rerenders; performance testing.
- **Product** — Note/Event types, dates/times, filtered views, font size, color themes.

See [`notes/DESIGN.md`](notes/DESIGN.md) §5–7 for the longer version.

## Further reading

| Doc | What's in it |
|---|---|
| [`notes/DESIGN.md`](notes/DESIGN.md) | Trade-offs, thought process, scalability, future work — **start here for explanation notes** |
| [`notes/PRD.md`](notes/PRD.md) | Product requirements |
| [`notes/Log.md`](notes/Log.md) | Day-by-day work log (chronological, unpolished) |
| [`notes/chats/`](notes/chats/) | Research rabbit holes (DB, auth, UI libs, etc.) |

