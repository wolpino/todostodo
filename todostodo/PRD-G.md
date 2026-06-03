# Product Requirements Document — todostodo

---

## 1. Project Purpose

A simple, accessible to-do web application that is enjoyable to use. The interface is clean and uncluttered, responds to window size and mobile screens, and lets users customize the look to their preference. The goal is a tool that feels as natural as writing on an index card — everything in one list, always at hand.

---

## 2. Tech Stack

| Layer | Technology |
|---|---|
| Backend | C# / ASP.NET Core (.NET) |
| Database | SQLite |
| Auth | ASP.NET Core Identity + JWT bearer tokens |
| Frontend | React + TypeScript |
| Component library | Chakra UI |

---

## 3. User Stories

### Authentication
- As a user, I can create an account with a username, email, and password.
- As a user, signing up automatically logs me in — I am not redirected to a separate login step after registration.
- As a user, I can log in to my account with my username or email and password.
- As a user, my session is maintained via a JWT token so I do not have to log in on every visit.

### Todo List — Default View
- As a user, my default view after logging in is my list of todo items.
- As a user, when my list is empty I see one empty row so I can immediately start adding items.
- As a user, I can click on an empty row to reveal an input field and type a new todo item.
- As a user, when I click away from an input field the value is saved and displayed as plain text in place of the input.
- As a user, when I click on a saved item the input field reappears pre-filled with the existing description so I can edit it.

### Todo Item Fields
- As a user, each todo item displays: a status indicator, a description, an optional due date, and an optional due time.
- As a user, I can click a calendar icon on an item to open a date picker and select a due date; the selected date is then displayed in place of the calendar icon.
- As a user, I can click the date display again to change it.

### CRUD Operations
- As a user, I can create a todo.
- As a user, I can view a single todo.
- As a user, I can view all of my todos.
- As a user, I can update a todo's description, due date, due time, or status.
- As a user, I can delete a todo.

### Personalization
- As a user, I can customize the color scheme of the application.
- As a user, my settings (color scheme, font) are saved to my account and persist across sessions.

---

## 4. Key Features — MVP

### Inline Todo Editing
- Each row has a bullet-style status button on the far left.
- To the right of the bullet is the description input / display area.
- To the right of the description are icon controls: a calendar icon for due date and a clock icon for due time.
- A delete control (red ✕) appears on the right edge of each row.

### Date Picker Behavior
- Clicking the calendar icon opens a date picker inline.
- Past dates are disabled by default (a future Event entry type will re-enable them — see Post-MVP).
- Once a date is selected it is displayed next to the icon; clicking it again re-opens the picker.

### Status Indicator
- Each item has a status: `Active` or `Inactive`.
- The bullet button on the left toggles the status.

### Settings
- Per-user settings record stores `ColorScheme` and `Font`.
- Color scheme customization is available from within the app.

---

## 5. Non-Functional Requirements

- **Responsive design** — layout adapts to desktop, tablet, and mobile screen sizes.
- **Accessibility** — built with Chakra UI's built-in accessibility features (ARIA attributes, keyboard navigation).
- **Performance** — list renders smoothly; avoid unnecessary re-renders (use `React.memo` on pure components).
- **Security** — all todo endpoints require authentication; queries are always scoped to the authenticated user's ID.
- **Data integrity** — todo items are owned by a user and cannot be read or modified by another user.

---

## 6. Data Model

```
Entry (base)
├── Id: int
├── Description: string
├── Status: EntryStatus  (Active | Inactive)
├── CreatedDate: DateTime?
├── ModifiedDate: DateTime?
└── ApplicationUserId: int  → FK to ApplicationUser

Todo : Entry
├── DueDate: DateOnly?
└── DueTime: TimeOnly?

Settings
├── Id: int
├── ColorScheme: string
├── Font: string
└── ApplicationUserId: int  → FK to ApplicationUser
```

> **Implementation note:** `Entry`, `Todo`, and future entry types share a single `Entries` table using EF Core's table-per-hierarchy (TPH) pattern with an `EntryType` discriminator column.

---

## 7. Post-MVP Features

- **Time input** — clicking the clock icon reveals a time input; selected time is displayed next to the icon.
- **Complete animation** — clicking the bullet marks the item complete with a green checkmark; optional confetti on completion.
- **Swipe to complete** — swipe gesture on mobile to cross off an item.
- **Smart sorting** — items are sorted automatically into relevant groups (groceries, books, reminders, etc.) based on content.
- **Subtasks** — a `Subtask` entry type with a `ParentId` FK pointing to its parent `Todo`.
- **Notes** — a `Note` entry type with a `Type` field (e.g. `grocery`, `media`) for richer list management.
- **Events** — an `Event` entry type with `Date`, `Time`, and `Location`; events allow past dates in the date picker.
- **Timezone support** — timezone saved on account creation; times stored in UTC and displayed in the user's local timezone.
- **Filtered views** — pre-built queries: *Due today*, *Due this week*, *Past due*.

---

## 8. Out of Scope (for now)

- OAuth / social login (code is stubbed in `Program.cs` but commented out)
- Sharing todos between users
- Push notifications or email reminders

---

## Optional Suggestions

### S1 — Split `Settings` into its own endpoint rather than embedding in auth responses
*Why it's better:* Settings will grow (timezone, notification prefs, etc.). A dedicated `GET /api/settings` + `PUT /api/settings` endpoint keeps auth clean and makes settings independently cacheable on the frontend.

### S2 — Add `CompletedDate: DateTime?` to `Entry`
*Why it's better:* The PRD mentions a completion animation and a future *Past due* filter. Storing when an item was completed makes those features trivial to implement and enables a completion history view without a separate log model.

### S3 — Consider `Completed` as a third `EntryStatus` value instead of `Inactive`
*Why it's better:* `Inactive` is ambiguous — it could mean "hidden/archived" or "done." A three-value enum (`Active`, `Completed`, `Inactive`) maps directly to the UI states (in progress / done / archived) and avoids overloading a single status value for two different user intents.
