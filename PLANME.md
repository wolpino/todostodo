# Todos To Do > TTD

a todo list management application

## Tech Requirements

**backend api:** C# and .NET Core
**database:** SQL Lite or EF Core in memory
**frontend:** React and Typescript

## Nonfunctional requirements

- mobile friendly/ready // responsive

## MVP Functionality

- CRUD todos 
- full test coverage
- logging
- error and exception handling

## MVP Production

*what does "done" mean*

- scalability
- visual design
- accessibility

## Ideas

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

---

## FEATURES/FUNCTIONALITY

- relational todos: like jira tickets, can link to other tasks > related to, duplicates, parent of, depends on, do before, etc

## Decisions

*simple* but not the *same*  

Index Card - Bullet Journal > what's the middleground?

## schema
Task
- id
- title/task description (max length?)
- extra description
- created date
- modified date
- due date?
- status
- parentID nullable 
- dependencies?

User

Task

Views 

