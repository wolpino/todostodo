## FluentValidation

No, you shouldn't add it — it would be overkill for this app. FluentValidation earns its weight when you have:

- **Cross-field rules** (`EndDate` must be after `StartDate`)
- **Conditional validation** (field X is required only when Y is set to Z)
- **Business-rule validation** (can't archive an entry that was never Active)
- **Reusable validators** shared across multiple request types

None of those apply here. The two request DTOs each have two simple annotations. DataAnnotations + `[ApiController]` handles that perfectly and with much less boilerplate. The rule of thumb: reach for FluentValidation when your validation logic would require `if` statements inside the attribute or you need to express intent that reads like a sentence.

**What is missing** regardless of library:
- `UpdateEntryRequest` with both `Title = null` and `Status = null` is technically a no-op update — the controller accepts it and returns 204 having saved nothing. Not a crash, but worth a `[AtLeastOnePropertyRequired]` custom annotation or a FluentValidation rule if this bothers you.
- Invalid `EntryStatus` string values (e.g. `"Blah"`) fail during JSON deserialization and get returned as a 400 by `[ApiController]`, but the error message is a raw serializer exception, not a clean "valid values are: Active, InProgress…" message.

---

## Exception handling

The global handler approach is correct — you should **not** add more try/catch blocks in the controllers. Scattering try/catch everywhere creates inconsistent error shapes, hides bugs by catching too broadly, and adds noise. The middleware catches everything as a safety net.

There is one real bug in `Program.cs` though — the handler is registered **twice**:

```99:99:src/todostodo.api/Program.cs
app.UseMiddleware<GlobalExceptionHandler>();
```

`AddExceptionHandler<GlobalExceptionHandler>()` + `app.UseExceptionHandler()` is the correct registration. The `UseMiddleware` call on line 79 is redundant at best and can cause unexpected middleware-pipeline behaviour — `GlobalExceptionHandler` implements `IExceptionHandler`, not `IMiddleware`, so it doesn't have the `InvokeAsync(HttpContext)` signature the middleware convention expects. Remove line 79.

**What's genuinely unhandled right now:**

| Scenario | Current result | Better result |
|---|---|---|
| Two users update the same entry simultaneously | `DbUpdateConcurrencyException` → 500 | 409 Conflict |
| DB unavailable / query timeout | `SqliteException` → 500 | 503 Service Unavailable (with retry hint) |
| `SaveChangesAsync` constraint violation | Exception → 500 | Could be 409, but not worth it yet |

For a single-user app in its current state, none of these matter in practice. The concurrency one becomes relevant as soon as you add multiple users with shared entries.

**The pattern to use when you do need differentiated errors** — instead of try/catch in the controller, extend `GlobalExceptionHandler` to branch on exception type:

```csharp
public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
{
    var (status, title) = exception switch
    {
        DbUpdateConcurrencyException => (409, "The resource was modified by another request."),
        _ => (500, "An unexpected error occurred.")
    };
    // write ProblemDetails with that status/title
}
```

That keeps error handling in one place, keeps controllers clean, and gives clients accurate status codes.