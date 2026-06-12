TPH is the strategy EF Core uses when multiple C# classes that share an inheritance hierarchy are all stored in a single database table. Your codebase is a great example — let me use it directly.

---

## The C# side

You have this class hierarchy:

```csharp
Entry          // base class
└── Todo       // inherits from Entry
```

`Todo` inherits all of `Entry`'s properties (`Id`, `Description`, `Status`, `CreatedDate`, etc.) and adds its own (`DueDate`, `DueTime`).

## The database side

Instead of creating two tables (`Entries` and `Todos`), EF Core creates **one** table with **all columns from every class in the hierarchy**:

```
Entries table
┌────┬─────────────┬────────┬─────────────┬──────────────┬──────────────┐
│ Id │ Description │ Status │ CreatedDate │ DueDate      │ DueTime      │ EntryType │
├────┼─────────────┼────────┼─────────────┼──────────────┼──────────────┤
│ 1  │ "Buy milk"  │ Active │ 2026-06-03  │ 2026-06-04   │ 09:00        │ "ToDo"    │
│ 2  │ "Meeting"   │ Active │ 2026-06-03  │ NULL         │ NULL         │ "Entry"   │
└────┴─────────────┴────────┴─────────────┴──────────────┴──────────────┘
```

The key ingredient is the **discriminator column** — `EntryType` in your case. EF writes a string into that column to record which C# type each row represents. When it reads a row back, it checks the discriminator and constructs the right object.

## Where it's configured in your code

```27:27:todostodo/Data/ApplicationDbContext.cs
            .HasValue<Todo>("ToDo");
```

```24:27:todostodo/Data/ApplicationDbContext.cs
        builder.Entity<Entry>()
            .HasDiscriminator<string>("EntryType")
            .HasValue<Entry>("Entry")
            .HasValue<Todo>("ToDo");
```

This tells EF:
- Use a column called `EntryType`
- When you see `"Entry"` → create an `Entry` object
- When you see `"ToDo"` → create a `Todo` object

## Why columns from subclasses are nullable

`DueDate` and `DueTime` only belong to `Todo`, but since base `Entry` rows also land in the same table, those columns **must be nullable at the database level** — base `Entry` rows have no value to put there. You can see this in your migration:

```26:27:todostodo/Migrations/20260601171142_RenameTaskModelToTodo.cs
                    DueDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DueTime = table.Column<TimeOnly>(type: "TEXT", nullable: true)
```

## Querying

EF automatically adds a `WHERE EntryType = 'ToDo'` filter when you query through `context.ToDos`, and no filter when you query through `context.Entries` (which returns all types). You never write that filter yourself.

---

## TPH vs. the alternatives

| Strategy | Tables | How it works |
|---|---|---|
| **TPH (what you have)** | 1 | All types in one table + discriminator column |
| **TPT (Table-per-Type)** | 1 per class | `Entry` table + `Todos` table joined on PK |
| **TPC (Table-per-Concrete-type)** | 1 per leaf class | Only concrete classes get tables, no joins needed |

**TPH is the EF Core default and usually the best choice** when:
- Your subclasses don't have many unique columns (yours have two — ideal)
- You query across the hierarchy often
- You want simple, fast queries (no joins)

The trade-off is wasted `NULL` space for subclass-only columns, which is negligible at small-to-medium scale. TPH becomes awkward only if a subclass has dozens of unique columns or strict NOT NULL requirements on those columns.