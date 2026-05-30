

>> Design
>> Nonfunctional requirements
>> Features


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