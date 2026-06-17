# Product Requirements Document — todostodo

Out of date — see DESIGN.md and code

---
## 1. Project Purpose

A simple, accessible to-do web application that is enjoyable to use. The interface is minimal and easy to use, responds to window size and mobile screens, and lets users customize the look to their preference. The goal is a tool that feels as simple and straightforward as writing a to do list by hand.

---

## 2. Tech Stack

| Layer | Technology |
|---|---|
| Backend | C# / ASP.NET Core (.NET) |
| Database | SQLite |
~~| Auth | ASP.NET Core Identity + JWT bearer tokens |~~
| Frontend | React + TypeScript |
| Component library | Chakra UI |

---

## 3. User Stories

### Authentication/Login/Signup
- As a user, I can create an account with a username, email, and password.
- As a user, signing up automatically logs me in — I am not redirected to a separate login step after registration.
- As a user, I can log in to my account with my username or email and password.


### Todo (Entry) Item - component displaying a Todo
- Each todo item displays: a status indicator and a description (A rough wireframe is at @uglywireframe.png item B.)
- As a user, when I click away from the description input text field the value is saved and displayed as plain text in place of the input.
- As a user, when I click on a saved item the input field reappears pre-filled with the existing description so I can edit it.

### Todo List — made up of Todo Items
- As a user, my default view after logging in is my list of todo items.
- As a user, when my list is empty I see one empty row so I can immediately start adding items.
- As a user, I can click on an empty row to reveal an input field and type a new todo item.

### Personalization
- As a user, I can customize the font of the application by selecting one of the provided fonts.
- As a user, my settings (font) are saved to my account and persist across sessions.

---

## 4. Key Features — MVP

### Inline Todo Editing
- Each row has a bullet-style status button on the far left.
- To the right of the bullet is the description input / display area.
- A delete button (a heavy lined X) appears on the right edge of each row.
- When there are no Todo Items to list, the default display is an empty Item. It looks similar to other Todo Items, but has it's own bullet (which is not a status.)

### Status Indicator
- Each item has a status: `Active`, `In Progress`, `Completed`, `Archived`, or `Inactive`
- The button on the left toggles the status, each click switches to the next status in order of the above list. 

### Settings
- There is a gear icon button at the top left of the page when clicked a dropdown list with the font options displays and when a user clicks on an option, the dropdown closes, and the font changes.
- Per-user settings record stores `Font`.

---

## 5. Non-Functional Requirements

- **Responsive design** — layout adapts to desktop, tablet, and mobile screen sizes. (What browsers are necessary for MVP? Chrome, Edge, Safari? Is anyone still doing Internet Explorer? I hope not.)
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
├── Status: EntryStatus  (Active | Inactive) ==> change to `Active`, `In Progress`, `Completed`, `Archived`, or `Inactive`
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

>>> **Implementation note:** `Entry`, `Todo`, and future entry types share a single `Entries` table using EF Core's table-per-hierarchy (TPH) pattern with an `EntryType` discriminator column.

---

## 7. Post-MVP Features


### add Entry types: Event, Note, Grocer
-- user can create different types of entries
-->> OR auto tag

- Color scheme customization is available from within the app.

### Date Picker Behavior
- Clicking the calendar icon opens a date picker inline.
- Past dates are disabled by default (a future Event entry type will re-enable them — see Post-MVP).
- Once a date is selected it is displayed next to the icon; clicking it again re-opens the picker.

- **Time input** — clicking the clock icon reveals a time input; selected time is displayed in place of the icon and is still clickable to edit. And some way to clear the input

- **Swipe to complete** — swipe gesture on mobile to cross off an item.

- **Subtasks** — a `Subtask` entry type with a `ParentId` FK pointing to its parent `Todo`.


- **Notes** — a `Note` entry type with a `Type` field (e.g. `grocery`, `media`) for richer list management.
things you don't want to forget
facts, ideas, thoughts, observations > capture data and information you don't want to forget. 
- *Susan is lactose intolarent*
- *wombats have square poop*

**Events** — an `Event` entry type with `Date`, `Time`
- short objective, date related entries
- *Jane's anniversary*
- *walked home to clear head*
- *signed the lease*

- **Timezone support** — timezone saved on account creation; times stored in UTC and displayed in the user's local timezone.
- **Filtered views** — pre-built queries: *Due today*, *Due this week*, *Past due*.

---

## 8. Out of Scope (for now)

- OAuth / social login (code is stubbed in `Program.cs` but commented out)
- Sharing todos between users
- Push notifications or email reminders

---


