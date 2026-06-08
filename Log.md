

TODO NEXT
> Double check non functional requirements are all MVP, also if any are missing
> Make comprehensive to do (haha) list for whatever Needs to be done, but then go through it and figure out what does not actually need to be done. 



# 6/3

Today is time for solidifying my next steps and full plan. Usually I'd do this earlier, but I wanted to leave things kind of open for more chances to learn.
> moving into Cursor and exploring the current project structure led to a few comments I want to understand more or fix, now, before I go further.


Tasks in response to project structure exploration:

"there is no Startup.cs — the app uses minimal hosting in Program.cs only." 
- Should there be a Startup.cs file? Why is it necessary? Where does it go and what should it look like?

"JWT key is expected via user secrets (dotnet user-secrets set JWT:Key "..."), not in appsettings.json."
- Add a ADDKEYHERE jwt key to appsettings.json

"SQLite in-memory (Data Source=:memory:) at runtime in Program.cs.
No Database.Migrate() or EnsureCreated() in startup."
- Would Database.Migrate() or EnsureCreated() go into the startup file? is this the best way to initiate an in-memory SQLite database?

"Auth/authorization middleware is registered after MapControllerRoute / MapFallbackToFile (unusual ordering)."
- What is the usual ordering? Does this have any affect on the code?

"Gap: TasksControllerHappyPathTests references a DatabaseFixture class that does not exist in the repo (no class DatabaseFixture anywhere). Tests expect _databaseFixture.InitializeAsync() and .DbContext."
- rewrite TasksControllerHappyPathTests to include all necessary references and expectations

"README documents DB setup as: dotnet ef database update (file-based SQLite via migrations), which differs from the in-memory connection string in Program.cs."
- How do I fix this? I want in-memory Sqlite for the database. Is it possible to use entity framework along side in-memory Sqlite

Neither file defines connection strings, JWT, or OAuth settings — those come from user secrets (JWT:Key on the project) and commented OAuth config in Program.cs.
- which files? should they have connection strings, JWT, and OAuth settings? show me an example based on best practices in C#/.NET web applications (from https://learn.microsoft.com/en-us/dotnet/csharp/ )

--

I made a @uglywireframe.png which made me realize the todo list is functional without the date and time, but not with out the Todo and the Status. so I'm going with design B, please please do not judge me on this wireframe.
I thought about leaving the settings for after the MVP too, but I've liked the idea of it being customizable from the beginning. Both of these options SEEM relatively straight forward, but I'm suspicious of that. 
This would typically be a internet search (probably something like 'implement settings in react app') but I caught myself and am going chat instead. 
-- There's a way to add color themes with Chakra's dark/light toggle that uses Semantic tokens which is interesting but possibly (probably) has a high risk for error.
-- adding fonts is simpler to implement so I'm going with that for initial MVP so I can have a setting, but it won't slow things down too much (hopefully).

>> PRD is looking less messy. 


# 6/2
I'm doing some Cursor learning because it was very easy to hit the credit limit, and also my prompting hasn't been giving me what I want. I struggle sometimes with tech vocabulary. I don't know if it's a working memory thing, but I don't think I was explicit enough. But also not having the vocabulary is definitely a cop out.
> regardless, I have more confidence explaining the frontend probably because I visualize in words// not images. sure.

https://blog.logrocket.com/frontend-devs-heres-how-to-get-the-most-out-of-cursor/

- problems and fixes file?
- rename ToDo to Todo

# 6/1

I've had some trouble getting the right tests created, partly because of ambiguous Tasks, my model and an async Task, which came up before, so I changed the Task model to ToDo. I tried this before with copilot and it was not good. I missed a few Todos that should be ToDo, but letting it go for now.

I need tests and a frontend.

Ooooookay, fool me twice copilot and I'm the fool. I'm not familiar enough with C# testing to give a better prompt and I've generated two unbuild-able tests. But it did remind me why there's the project and the solution so that is fixed and I'm just going to add the tests myself. This is what I want it to be good at! 
*note* how to generate good tests??

Also my computer turned back on. Just incase whomever is reading this was waiting for an update.

I have blown through my free credit limit in vsc, but that's great cause now I can try out Cursor for the frontend. 

But first I'm looping back to my Plan doc. I like starting with the backend it's easier for me to hold it all in my head when its front to back linking. Going to play with some designs in excalidraw and work on a prompt/spec sheet

# 5/31

- make a skill to save summaries from chat
- With in memory do you need to check availability memory

copilot got stuck in a weird loop trying to fix the 
---

adding tests...
I do not remember much about C# testing. I didn't get into a groove with testing until I used Python. so deciding which framework to use

fixing issues
https://stackoverflow.com/questions/48061096/why-cant-i-call-the-useinmemorydatabase-method-on-dbcontextoptionsbuilder


--
Adding in fluentvalidation... it is almost too much for the CRUD app as it is... but not?
>> I remmeber something about migrations sometimes causing issues but it's vague, and that's why I've been holding on creating and migrating the migrations, but I need to do it soon...
I wasn't soon enough.


`➜  todostodo git:(testing-plus) dotnet-ef migrations add` AddTasksAndSettingsModels
Build started...
Build succeeded.
An error occurred while accessing the Microsoft.Extensions.Hosting services. Continuing without the application service provider. Error: JWT:Key configuration is missing
Unable to create a 'DbContext' of type 'ApplicationDbContext'. The exception 'Unable to resolve service for type 'Microsoft.EntityFrameworkCore.DbContextOptions`1[todostodo.Data.ApplicationDbContext]' while attempting to activate 'todostodo.Data.ApplicationDbContext'.' was thrown while attempting to create an instance. For the different patterns supported at `design time, see https://go.microsoft.com/fwlink/?linkid=851728`

-- easy fix adding a jwt "buffy-4-lyfe"
but I remember I wanted to look at the models before migrating because I'm not convinced on some field names >> overthinking it gotta move forward

>> AI things I like,
- I can confirm if my plan is correct and then also get more context
that is helping solidfy things in my mind



# 5/30
the following is the prompt I used to generate the backend api. 


add these models and build an api to connect to the ClientApp. The API should have basic CRUD actions for the Task model

class Settings
int Id
color scheme
font
?
FK ApplicationUserId

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
---
The code that produced was a decent structure but I'm going through making adjustments now. 
--
I ran out of rate limits, which reminds me I need to better understand all of everything and why. yikes. but also I got sucked in!
But besides that, wow! that felt like the first time understanding the workflow. someone said it was like a junior engineer, and it is?
asking for specific things feels like crafting a slack message that's clear and communicates the correct thing. 
*I saved the chat and will need to read back through it...*
/chats/fullsession: CRUDapi.md

# 5/29
Had an incident with some water and my laptop today. Luckily committed almost all of the work (but not my Log -- so if something necessary could be missing...) But had to set up my other laptop to use dotnet

Taking space from all the ideas was good, now I can go simple instead of too broad of a scope. 

I'm going to reread the prompt and finalize my plan doc, specifically MVP

# May 28
>>added SQlite
I'm choosing EF Core + Sqlite for LINQ, migrations and Identity integration

The project is way out of date with the dotnet I downloaded but thats ok because of modernize!
https://devblogs.microsoft.com/dotnet/modernize-dotnet-anywhere-with-ghcp/ 
>> this was successful, and entirely done by an AI agent

# 5/27
tried to get project running before adding sqlite. remembered about Solutions vs projects so I'm going to see if I can fix this without making a new project.
-- the issue was 
also the HTTPS Developer Certificate was causing the Error, the suggested commands worked.... after a restart.
https://stackoverflow.com/questions/54371101/cannot-create-developer-certificate-on-mac
https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/9/certificate-export?view=aspnetcore-10.0



# 5/26

## generate framework for react/.net core application
- authentication built in
- I haven't used C# for 5 or 6 years, so want to review and also need to set up VSC => added dotnet, added a new template
`dotnet new search <searchterm>`
`dotnet new install Asp.React.Project.Templates`

## Planning/brain dump/define MVP
- first pass / all the thoughts / mess > braindump.txt
- PLANME.md >> clean up of initial pass