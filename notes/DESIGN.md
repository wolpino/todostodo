# Design notes — todostodo

**What this file is:** polished explanation notes for reviewers.  
**What it is not:** a day-by-day log — that's [Log.md](Log.md).  

---

## 1. Assignment interpretation

`Build a small to-do task management API and frontend.` 

I interpreted this to be a todo list application, with additional features to manage the tasks in the list. I need users to be able sto create,update,read and delete items that contain tasks. Items should have some sort of status lifecycle (to do turning to done). My own experience with todo applications is they are too complicated and busy, or take lots of clicks to write or edit items. So app use has to be simple with minimal clicks.

- `Backend API design — .NET Core`  
 I hadn't worked with or really looked at C# code for 5 years. But I saw this as a great use case for AI tools that I've been wanting to work with.  (more on that later)
- `Data structure design — SQLite or EF Core in memory`  
 I didn't like that EF Core InMemory didn't enforce foreign keys, or validate SQL. Whereas SQLite in memory does enforce foreign keys (not by default), constraints, unique indexes and it validates SQL properly. Buuuut, I am using EF Core for how I talk to the data - I didn't want to write raw sql queries or build my own schema, plus change tracking, Identity integration.
- `Frontend component design — React or Vue`  
 I've worked with React before but not Vue. Since C# was going to require a lot of review, it was the easier options.
- `Communication between frontend and backend` — see §2
- `Production MVP features — see §6`
 Authentication (register, login, logout) >> data should only be shared when the user is aware and desires it. 
 CRUD-ish entries with status lifecycle
 Tests (API + frontend)
 Docker + CI >> ability to run easily and reliably
 Exception handling and logging - a production app shouldn't blow up when something goes wrong, exceptions need to be handled appropriately, logging is needed for debugging
 While I also consider performance testing an important part of maintaining production applications - it's not needed for this MVP

---

## 2. Thought process

### .Net/C#/CoPilot/Cursor

I've been working in Python for the last 5 years and while I think C# made a good foundation to use with Python (thinking OOP) I wasn't remembering much. So I needed to review C#, but I was also eager to see what I could do with Copilot. Because I learn fastest by doing and seeing what fails and what doesn't, I dove in without review so I could start looking at code, and use Copilot to deal with all the C# questions.

  
It ultimately helped me review a lot of C# which came back easier than I expected. BUT, I think I remembered enough C# to remind me how much I don't know, and therefore can't easily check the generated code.  Which means I found myself in a messy repo that didn't work. While I hesitated at first, after some crazy looking generated tests, I wiped the whole project to start with a different approach. 


I do this when drafting stories, my thought patterns are really web like, so I can get a big picture but it comes from smaller details. So moving to a clean page/clean repo helps me reset my focus and decide how to move forward. 
I used VSC to set up the project and get scaffolding and general understanding in the simplest changes possible. (this was because I'd read a number of posts about people having issues with .Net in Cursor), I also knew I'd understanding it best if I went through the foundational steps. And set up testing and tests.


Moving back to Cursor I added rules for .Net and react and Vite, and spent time working out the plan for the rest of the app. I also made a bad wireframe to have a visual to aim for, and understand what components to make. This is when I also decided I didn't need the ability to set time/date in the MVP. Useful, but functional without.

My approach with code generation this time was small pieces and changes, made with the intention of using small commits and pull requests for larger features or collections of changes. This made it much easier to not only keep track of all changes, but review them immediately in small chunks. This worked well for me.

### Status lifecycle

The status button cycles forward only: Active → InProgress → Completed → Archived, then stops. I didn't want it looping back to Active — that felt too easy to accidentally undo something you meant to finish. 

Inactive is separate: that's delete. Soft delete, so the row disappears from GET but the row still exists if I want trash/restore later.

Sort order: InProgress floats to the top, Archived sinks to the bottom. Everything else stays in place. Simple way to show "what I'm working on" without building filtered views yet.

### API style

Typed REST API
Typed for safety and REST because it's simple and a good match for a CRUD app.

### Auth

Cookie-based auth, generated client + React Query. I wanted real serverside logout! which JWT does not offer. Also cookies kind of came for free with Identity API

### Architecture

Decoupled SPA + API.

### Protocol & format

- HTTP/HTTPS with JSON bodies
- REST-style endpoints, e.g. `GET /api/Entry`, `POST /api/Entry`, `PUT /api/Settings`
- Auth via ASP.NET Identity API (`/login`, `/register`, `/manage/info`, `/logout`)

### Client ↔ server wiring

1. OpenAPI contract — API exposes Swagger; frontend runs `pnpm generate-api` → typed clients in `src/api/generated/`
2. `fetch` — generated client uses `@hey-api/client-fetch`
3. Cookie sessions — `useCookies: true`; browser stores auth cookie and sends it on every request
4. TanStack Query — `useEntries`, `useSettings`, `useAuth` wrap the client for cache, loading, optimistic updates

---

## 3. Trade-offs


| Decision                | Chosen                                            | Alternative                      | Why                                                                                                                                                                                                                                                                                                                                                                       |
| ----------------------- | ------------------------------------------------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Data model              | Flat `Entry` + `EntryKind`                        | TPH inheritance                  | I liked the idea of TPH inheritance, but ultimately I realized I didn't need it for the MVP. A single Entries table with EntryKind + nullable subtype columns + separate tags matches my goals with less complexity and the same scaling story.                                                                                                                           |
| Auth                    | Cookie sessions (Identity)                        | JWT bearer                       | I wanted real serverside logout! which JWT does not offer. Also cookies kind of came for free with Identity API                                                                                                                                                                                                                                                           |
| Database                | SQLite in-memory                                  | EF Core in memory                | I'm significantly more familiar with SQLite, and it seemed like less complexity and offered real constraints and SQL validation                                                                                                                                                                                                                                           |
| API shape               | REST + OpenAPI codegen                            | GraphQL                          | Typed for safety and REST because it's simple and a good match for a CRUD app... graphQL is too complex for the app                                                                                                                                                                                                                                                       |
| Delete                  | Soft delete (`Inactive`)                          | Hard delete / trash              | Is this necessary? Maybe not. but I had the thought it could be useful to see deleted todos, either to duplicate or remember you did the task.                                                                                                                                                                                                                            |
| Kind on create          | Set once, not editable in MVP                     | User can change kind later       | I wanted to set up the Kind in the Entry table and have the initial set up to show the future flow. It will be fully implemented later, when there are more kinds.                                                                                                                                                                                                        |
| Settings API            | Dedicated `GET/PUT /api/settings`                 | Fold into user profile only      | Settings will grow (timezone, notification prefs, etc.). A dedicated endpoint keeps auth clean and makes settings independently cacheable on the frontend.                                                                                                                                                                                                                |
| UI library              | Chakra UI                                         | Material UI                      | I wanted to use a frontend library. Being unsure which one, I looked at reddit posts/articles on preferences and landed on Chakra. I wasn't planning to use TanStack Query but it was included in a list of TS rules/settings so I looked into it and it actually ended up simplifying caching, allowed for optimistic updates, and was able to manage the loading state. |
| Multi-window UX         | Pop-out button (~440×720) + BroadcastChannel sync | Resize browser / single tab only | Adjusting the user's browser window was more difficult and ultimately less reliable than opening a new window. Sync is best-effort — last write wins if you edit both tabs at once.                                                                                                                                                                                       |
| Settings close behavior | Manual close on desktop (X)                       | Close on outside click           | I wanted users to see the font change on the list while picking a font. Annoying, but intentional.                                                                                                                                                                                                                                                                        |


## 4. Assumptions

Things I assumed true for this build:

- Single user, first-party web app (no sharing between users yet)
- In-memory DB is acceptable for demo even though data does not survive restart
- Cookie auth needs same-origin or dev proxy
- Register auto-logs-in; I assumed that's fine for a personal todo app
- Users will have a small list size — no pagination or virtualization yet
- Desktop-first narrow window; mobile works but not primary design target for MVP

---

## 5. Scalability

Fine for a demo and small personal lists; first pain point is unbounded `GET /api/Entry` and in-memory data loss on restart.
Not ready to handle long lists — will need pagination on `GET /api/Entry`; filtering and sorting by status and dates. I think a max list length might be useful and reasonable.

**Data / persistence**

I'd move to file-backed SQLite or Postgres, add EF migrations, index on `UserId` and `Status`. Right now `EnsureCreated()` at startup is fine for demo; it doesn't survive restart.

**API**

Pagination and filters on `GET /api/Entry` as lists grow. Sorting by status/date is already partly handled on the frontend; at scale that belongs on the server.

**Auth (if multi-instance)**

Cookie sessions work for a single API instance. If I scaled horizontally I'd need a shared session store (Redis or similar) — haven't built that.

**Frontend**

Virtualized list if there are thousands of rows; cut unnecessary rerenders. I called out performance testing as important but didn't do it for this MVP — I'd want that before calling it truly production-ready at scale.

**Ops**

Already have Serilog + global exception handler + CI. Next would be health checks, maybe retries on the client for transient failures.

---

## 6. Production MVP — what I shipped

Beyond the assignment basics, these are the things I considered required for something I'd actually call a production MVP:

**Auth**

- Register, login, logout via ASP.NET Identity (cookie sessions)
- Signing up logs you in — no extra redirect step
- Every entry query filters by UserId. I had an early bug where all users could see all tasks; fixing that was non-negotiable.

**Entries / task management**

- CRUD on entries (create, read, update, soft-delete)
- Status lifecycle: Active → InProgress → Completed → Archived. Clicking the status bullet advances one step; it stops at Archived (doesn't loop back). Delete sets Inactive and removes the item from the list.
- InProgress sorts to the top, Archived to the bottom — so "working on" and "done but visible" feel different without separate views yet.
- Composer row at the top: type and press Enter to add. That was a deliberate minimal-clicks choice.
- Kind is on the Entry table (`Todo` / `Note` / `Event`) but only Todo is in the UI. Kind update is disabled on the backend for MVP — there's a skipped test for that.

**Settings**

- Per-user font choice via `GET/PUT /api/settings`
- I initially used Caveat for a font choice but felt it wasn't readable at the set size so I switched to Patrick Hand
- Gear menu: sign out, font picker. Desktop popover, mobile bottom drawer.

**UI**

- Fixed-width notebook shell (~420px) — meant to sit beside other windows
- Pop-out button opens a ~440×720 window instead of asking users to resize their browser (that felt unreliable)
- Cross-tab sync so pop-out and parent tab don't stay totally out of date (`BroadcastChannel` + refetch on window focus)

**Ops / reliability**

- Docker one-command run from repo root
- CI: API tests, frontend tests, Vite build, docker compose build
- Serilog request logging + global exception handler — production shouldn't just blow up silently

**Not in MVP (but on my list)**

- Performance testing / cutting unnecessary rerenders
- Retries on failed API calls
- Trash / restore for deleted items

---

## 7. Future features

**Entry types & fields**

- Note, Event entry kinds (enum exists; UI not built)
- Date/time on todos (only future dates)
- Events with past dates allowed
- Accidental delete handling

**UX**

- Adding font size and color themes for customizability
- a fun confetti animation when task is completed.
- Swipe to complete on mobile

**Views & sorting**

- Smart sorting: if user adds a food item or book it would get tagged as groceries or books to read
- Filtered views: Due today, Due this week, Past due
- Previous day snapshot

**Account & notifications**

- Timezone handling(UTC storage, local display)
- In-app notification, or email notifications, weekly summary. Reminders/push notifications

---

## 8. Open issues / known quirks

*Honest list — better here than hidden.*

- Completed → Archived click regression (might be pop-out related) this is close to being fixed, but I think there are still some edge cases.
- Settings popover on desktop doesn't autoclose on outside click (necessary to allow for font preview but annoying.)
- Pop-out leaves parent tab open; cross-tab sync helps but last write wins if editing both
- Kind update is commented out in EntryController; test `Update_PersistsNewKind` is skipped intentionally for MVP

---

## Appendix — where to find more


| File             | Purpose                                 |
| ---------------- | --------------------------------------- |
| [PRD.md](PRD.md) | Initial product requirements            |
| [Log.md](Log.md) | Chronological work log (messy, genuine) |
| [chats/](chats/) | Deep dives (DB, auth, UI libs, etc.)    |


