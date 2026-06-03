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
-- ability to make multiple lists
-- duplicate a task 
- recurring tasks
-- sub tasks // events // notes
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
