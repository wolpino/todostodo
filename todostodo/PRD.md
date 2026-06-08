## Problem statement:
   
A simple, accessible, to-do web application that's enjoyable to use. It's dynamic and responsive to window size and mobile screens.  The design is simple and the colors are customizable.

## Tech Stack

**backend**: C# and .NET, sqlite  
**frontend:** React and Typescript  
**component library:** Chakra Ui

Features:


User stories

- As a user, I can login and sign up for an account. 
- The sign up flow logs me in on account creation.
- The default view is a list of Todo items. - each item has a status, description, due day, due time


user can see an empty to do list.
user can click on the first rectangle and the first item input is displayed

- that has a button that appears as a bullet point on the left most side, to the right of the bullet point button, there is an input field for the user to write the to do item. when the user clicks out of the input, save the value and display it in place of the input field. when use clicks on the item, the input field prefilled with the description is displayed in place of the saved 
- when the list is empty user sees the first uncreate item row

user can log them in
User can...

- create a todo
- read a todo 
- read all todos
- update a todo
- delete a todo


there's a bullet to the left of the input, then icons to the right. a calendar for the date. a clock for time. 
click on the calendar icon, calendar will pop up, user clicks on a day, and the date now displays were the calendar was or to right of icon (so user would click it again to edit? I think I like clicking in the same place)

> initially all previous dates will be disabled, but adding the Event type will enable it.


# post MVP

> click clock icon, input for time, then display time. 
> on the right is a red x.
> click the bullet to get a green check, swipe? to cross it off. confetti?


*(my grandfather used to carry an index card with names, reminders, etc)* 
Everything goes to one list, smart sort into specifics 
- *groceries to get — apples, dishwasher liquid*
- *books to read — Judy said to read The Emerald by? Or a link*


> how do datetimes work in C#
> handle locale (save timezone on account creation) > future feature change timezone/auto set timezone (*how do you do this?*)
> timezone on account, time saved 
`var specified = DateTime.SpecifyKind(meetingTime, DateTimeKind.Utc);`
**local time is server's locale not users**

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
