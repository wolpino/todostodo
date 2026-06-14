60/60 passing. Here's a summary of what changed and why each decision was made:

**Why constructor injection over method-level**

The controller already used constructor injection for `AppDbContext` — logging is the same kind of cross-cutting infrastructure. Method-level DI (`[FromServices] ILogger<T>` as an action parameter) is the right call when a heavy service is only needed by one action, so you avoid constructing it on every request. A logger is neither heavy nor occasional, so method-level buys nothing and instead adds noise to every method signature.

I also switched to **primary constructor syntax** (`EntryController(AppDbContext db, ILogger<EntryController> logger)`) — it's the modern C# 12+ pattern, consistent with `GlobalExceptionHandler`, and removes the boilerplate `private readonly` fields.

**What was logged and at which level**

| Situation | Level | Reasoning |
|---|---|---|
| Entry created | `Information` | Primary write event; core of any audit trail |
| Entry updated | `Information` | Audit trail for mutations |
| Entry deleted | `Information` | Always log deletions — irreversible and the first thing asked about in support |
| GET `/{id}` returns 404 | `Debug` | A user requesting a stale/deleted entry of their own is normal; `Warning` would be noisy |
| PUT or DELETE returns 404 | `Warning` | Mutating an entry that doesn't exist or isn't yours is suspicious — worth surfacing in monitoring |

**`NullLogger<T>.Instance`** is used in the two unit test files that construct the controller directly. It discards all log calls with zero overhead — the right default over mocking `ILogger` unless a test specifically needs to assert that a log message was emitted.