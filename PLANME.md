# Todos To Do > TTD
# Not Your Grandfather's Index Card
NYGIC
N-Y-Gick 

a todo list management application

> just a note, this md file is best viewed as code. 

## Tech Requirements

**backend api:** C# and .NET Core
**database:** SQL Lite or EF Core in memory
**frontend:** React and Typescript
**component library:** Chakra Ui
`npx skills add https://github.com/chakra-ui/chakra-ui/tree/main/skills/chakra-ui-builder`

## Functional requirements
User can...
- create a task
- read a task 
- read all tasks
- update a task (same as complete a task)
- delete a task

## Nonfunctional requirements
- full test coverage
- logging
- error and exception handling
- retries on api calls
- mobile friendly/ready // responsive
- security? login/signup?
- containerize

## MVP Production

*what does "done" mean*
- scalability
- visual design
- accessibility

Clear, straightforward architecture
Thoughtful decisions about what “production-ready MVP” means
Appropriate tests, logging, and security considerations
Clean, readable code
Sensible tradeoffs
Documentation of your thinking

# Approach
I want to experiment with various AI tools so I'm going to do a mix of my usual researching/understanding **(usually googling and looking at opinions on Stackoverflow or Reddit) > which starts by collecting data, make my own decisions** and pretend I'm pair programming with the chat...

- copilot to update packages
- copilot to generate backend scaffalding and tests
- chakra ui agent
- cusor to develop the frontend

# Design
mostly single page application 
-- The idea of going to a different page or even using a pop up on a list feels very clunky to me. 
-- first empty item and click to enter 
there's a bullet to the left of the input, then icons to the right. a calendar for the date. a clock for time. 
click on the calendar icon, calendar will pop up, user clicks on a day, and the date now displays were the calendar was or to right of icon (so user would click it again to edit? I think I like clicking in the same place)
> initially all previous dates will be disabled, but adding the Event type will enable it.
click clock icon, input for time, then display time. 
on the right is a red x.
click the bullet to get a green check, swipe? to cross it off. confetti?

- testdata, 5 users with 
- 
)


# Ideas
How can I remove any kind of blocker or reason to not do something, any accommodation for the recording of the task to be easier
Why is a pen and paper so easy? -- Write the list at once, seeing the other tasks. as seperate inputs? or one? or can get to the next with enter or tab/a key. 
The single input which creates the tasks, which the user can add to or update. >> so it functions like a bulk create

## Possible Features
-- timer
- ability to make multiple lists
-- duplicate a task 
- recurring tasks
- sub tasks // events // notes
-- multiuser collaborations(roles), 
- reminders/notifications - in application? emails, email weekly summery, ability 
- allow user to change default settings, theme/color, views etc
-- get an agent to break tasks into subtasks.
-- get agent to auto tag? 

-- archive todos, done or not >> tasks with no text, or less than two words get deleted. user gets prompted and they choose to remove. Hides the todo. (so can you unarchive? **why would you do this?**) so it doesnt display on the *one long list*, but you can see a list of archived tasks and make a copy (leave original in archive (**why**)).)
-- during the same day >you see the completed green check
-- the next day they show up bottom of active todo list, top of completed list which is  greyed out or strikethrough something > or just always hide/archive. 

>> Priority, assisted? is this more or less important than this? it help sort priority, goes through, is this more important than this, than add, is this more important, 
labeling/tagging > automatically? user picks from a list?(future feature, they can create custome tags.)

- sessions to save login auth?

*(my grandfather used to carry an index card with names, reminders, etc)* 
Everything goes to one list, smart sort into specifics 
- *groceries to get — apples, dishwasher liquid*
- *books to read — Judy said to read The Emerald by? Or a link*


# Schema
class ApplicationUser : IdentityUser

class Settings
int Id
color scheme
font
?
FK ApplicationUserId
**do I need this class/why not just add to the User?** 
>> I want to have a distinction between in app settings and user specific permissions, 

class Entry
int Id 
string Description (max length?)
DateTime? CreatedDate 
datetime? ModifiedDate
string *or enum* Status : active - inactive (defaut options, derived classes can add (or use a different enum?))
FK ApplicationUserId

class Task : Entry
DateOnly? DueDate
TimeOnly? DueTime

> how do datetimes work in C#
> handle locale (save timezone on account creation) > future feature change timezone/auto set timezone (*how do you do this?*)
> timezone on account, time saved 
`var specified = DateTime.SpecifyKind(meetingTime, DateTimeKind.Utc);`
**local time is server's locale not users**

# NOT MVP
class Subtask : Task
FK int ParentId 

class Note : Entry
string Type grocery - media (book/movie)

class Log ? 


class Event : Entry > scheduled plan
DateOnly? Date
TimeOnly? Time
string Location
string Details *(how is this differnt than desc?)*

Queries
- due today
- due this week
- past due

# Descisions

## Component Library 
Material UI comes to mind but I think there have to be better options > asked the agent chat and reddit
https://www.reddit.com/r/reactjs/comments/1cr53f0/chakra_ui_vs_material_ui/
/todostodo/chats/reactcomponentlibs.md
>> I'm going to add the Chakra Builder skill for speed.
`npx skills add https://github.com/chakra-ui/chakra-ui/tree/main/skills/chakra-ui-builder`


## Initial ideas:
i.e. letting myself think through whatever ideas I have so I don't get too excited and make it harder than necessary
###

### **SIMPLE TODO LIST**

*(direct interpretion)*
=> basic basic but iterate features like... multiuser collaborations(roles), reminders (notifications - in application? emails, email weekly summery, ability to set regular/recurring todos), due dates and times, current day todo list, one endless list, day/week status, nested/sub tasks

> future growth, add personas, specific features more helpful to: college students, engineers, high school student, family, etc, start basic, then inherit basic class/application to extend toward specilties

---

### **THE INDEX CARD**

*(my grandfather used to carry an index card with names, reminders, etc)* 
Everything goes to one list, smart sort into specifics 

- *groceries to get — apples, dishwasher liquid*
- *books to read — Judy said to read The Emerald by? Or a link*

---

### **ONE STICKY** (? Stick To It! // StickToDoIt ?)

*my most effective todos are on a brightly colored sticky note, with 5 todo*
items are ordered by priority, only the top 5 are shown. when you add a new one asks if it is more important than current 5, etc.

---

### **BULLET JOURNAL INSPIRED**

*there's a lot of manual migration, and while I think part of the point is to do it by hand, I find the collections useful ... but how is it more of a to do list than a journal/log? Inspired by not mirroring...*

### The Daily Log - > add todos/rapid logging

additional views > last week - this week - next week (3 days before/after)

#### this works as the todo list

- status
- type
- signifiers, different than status or type > CODE RED >> level of panic around the task shown by brightness of color

**ADD TODO FORM//types of entry**

- topic: ____  note: ____
 *Nicky: call re tomorrow*
- todone
*something already happened, already completed*
- note
*square poop*

#### *the migration is meant to surface whats worth the effort to rewrite it...so how to do it by finger not hand*

- evening reminder to take time to go over Daily Log and update it.  > save grabs a snapshot, and its easy to move what tasks you want forward, and what doesn't actually matter. 
- lists... future dates, grocery list, Month view, 
**click on bullet > list of options/status -- bullet is status image. red? green? grey? > by emoji? by color? symbol** Does it cycle through or show a list

---

## Research on bullet journals

- rapid logging:
- bullets are syntax > visually catagorize into 
(-) Tasks: things you have to do > 5 states
1. task incomplete
2. complete
3. task migrated to collection
4. task scheduled in future log

(O) Events: noteworth mentions in time
- short objective, date related entries that can be 1. scheduled or 2. logged after they occur.
- function to gather data >> future version include some visualization of data 
- *Jane's anniversary*
- *walked home to clear head*
- *signed the lease*
(--) Notes: things you don't want to forget
facts, ideas, thoughts, observations > capture data and information you don't want to forget. 
- *Susan is lactose intolarent*
- *wombats have square poop*


# Product Requirements Document — todostodo

---
>>> COMMENTS (from ari)
## 1. Project Purpose

A simple, accessible to-do web application that is enjoyable to use. The interface is clean and uncluttered, responds to window size and mobile screens, and lets users ze the look to their preference. The goal is a tool that feels as natural as writing on an index card — everything in one list, always at hand.

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
>>> I think this is right but double check in-member db and sessions
>>> - As a user, my session is maintained via a JWT token so I do not have to log in on every visit.

### Todo List — Default View
- As a user, my default view after logging in is my list of todo items.
- As a user, when my list is empty I see one empty row so I can immediately start adding items.
- As a user, I can click on an empty row to reveal an input field and type a new todo item.
- the Todo list is a collection of Todo Items

### Todo Item 
- As a user, when I click away from an input field the value is saved and displayed as plain text in place of the input.
- As a user, when I click on a saved item the input field reappears pre-filled with the existing description so I can edit it.

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
- As a user, I can ze the color scheme of the application.
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

>>> this needs to be updated to Active, In Progress, Completed, Archived, Inactive
>>>### Status Indicator
- Each item has a status: `Active` or `Inactive`.
- The bullet button on the left toggles the status.

### Settings
- Per-user settings record stores `ColorScheme` and `Font`.
- Color scheme customization is available from within the app.
- there is a settings page where user will be able to change s

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

>>> I did this wrong. I was too excited about inheritance I didn't give enough thought to how it should be in the db
>>> **Implementation note:** `Entry`, `Todo`, and future entry types share a single `Entries` table using EF Core's table-per-hierarchy (TPH) pattern with an `EntryType` discriminator column.

---

## 7. Post-MVP Features

- **Time input** — clicking the clock icon reveals a time input; selected time is displayed next to the icon.
>>> initial thought was display in place of the icon > do I still want that?
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
