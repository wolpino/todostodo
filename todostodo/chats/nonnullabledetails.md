## `null!` — the null-forgiving operator

`null!` is two things combined:

- **`null`** — the actual value being assigned
- **`!`** — the *null-forgiving operator*, which tells the compiler "I know this looks like it could be null, but don't warn me about it"

### Why it's needed here

The field is declared as a non-nullable reference type:

```csharp
private ApplicationUser _testUser = null!;
```

With `<Nullable>enable</Nullable>` on (which this project has), the compiler enforces that a field typed as `ApplicationUser` — not `ApplicationUser?` — can never hold null. If you just wrote:

```csharp
private ApplicationUser _testUser; // ❌ CS8618: Non-nullable field must contain a non-null value on exit from constructor
```

you'd get a warning/error because the field isn't initialized in the constructor.

You *could* write:

```csharp
private ApplicationUser? _testUser; // nullable — fine, but now every use needs a null check
```

But that's less clean when you *know* the field will always be set before use.

### The pattern: declare with `null!`, assign in `InitializeAsync`

```csharp
private ApplicationUser _testUser = null!;   // declared — compiler is satisfied

public async Task InitializeAsync()
{
    _testUser = MockUserFixture.CreateTestUser(userId: 1);  // assigned before any test runs
    ...
}
```

xUnit calls `InitializeAsync` (from `IAsyncLifetime`) before running any `[Fact]` method, so by the time a test touches `_testUser` it is always a real object. The `null!` is you telling the compiler: *"this starts null but I guarantee it won't be null when it matters — trust me."*

### When is `null!` appropriate vs. a smell?

| Situation | Good use of `null!`? |
|---|---|
| Field set in `InitializeAsync` / `SetUp` before tests run | Yes |
| Field set in constructor via DI | No — just initialize it there |
| You're not sure if it'll be set | No — use `?` and handle null properly |

It's essentially a way to defer initialization to a lifecycle method while keeping the type non-nullable everywhere else in the class.