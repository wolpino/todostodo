The rename changes only the C# class name, not the database schema.

The database stores rows in the Entries table with a discriminator column EntryType. The value written to that column is controlled by this line in ApplicationDbContext.cs:


ApplicationDbContext.cs
Lines 27-27
            .HasValue<Todo>("ToDo");
The string "ToDo" is deliberately preserved. Renaming the class from ToDo to Todo has zero effect on what gets written to the database — existing and new rows still get "ToDo" in the EntryType column.

If I had changed that string to "Todo", then a migration would be required, because any existing rows with "ToDo" as their discriminator value would become invisible to EF queries (it would only find rows matching the new string).

Also: this project uses in-memory SQLite.

DevDataSeeder.cs calls EnsureCreatedAsync() which builds the entire schema fresh from the model every time the app starts. Migrations are never applied — they exist in the project but aren't used at runtime. So even if the schema had changed, you'd just need to update the EF model, not write a migration file.

