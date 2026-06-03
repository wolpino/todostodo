## Problem statement:

##   
A Simple, accessible, to-do web application that's enjoyable to use. It's dynamic and responsive to window size and mobile screens.  The design is simple and the colors are customizable

 PRD: Define the app's purpose, features (posts, likes, follows, etc.), tech stack (.NET, SQL, CQRS, Swagger) 

## Tech Stack

**backend**: C# and .NET, sqlite  
**frontend:** React and Typescript  
**component library:** Chakra Ui



User stories





- As a user, I can login and sign up for an account. 
- The sign up flow logs me in on account creation.
- The default view is a list of Todo items. - The table columns are status - description - due day - due time
- the status is displayed as an image 
with an empty square check box
on load, user sees login/sign up page.
user can create an acount but the flow is smooth so on creation it logs them in.

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

The 

-- first empty item and click to enter 
there's a bullet to the left of the input, then icons to the right. a calendar for the date. a clock for time. 
click on the calendar icon, calendar will pop up, user clicks on a day, and the date now displays were the calendar was or to right of icon (so user would click it again to edit? I think I like clicking in the same place)

> initially all previous dates will be disabled, but adding the Event type will enable it.
> click clock icon, input for time, then display time. 
> on the right is a red x.
> click the bullet to get a green check, swipe? to cross it off. confetti?

Feature logic / specs

**frontend:** React and Typescript
**component library:** Chakra Ui

@ClientApp 
For now, stay in the  ClientApp directory as much as possible, if updates need to be made outside this directory, explain the code change to me to review before continuing. 