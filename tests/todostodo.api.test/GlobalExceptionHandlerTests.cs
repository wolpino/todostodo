using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using todostodo.api.Middleware;

namespace todostodo.api.test;

/// <summary>
/// Unit tests for <see cref="GlobalExceptionHandler"/>.
///
/// The handler is the single safety net for any exception that escapes a controller
/// action. These tests verify the three guarantees it must always uphold:
///   1. It claims every exception (returns true) so no other handler runs.
///   2. It always emits HTTP 500 — never leaks internal details via 200/4xx.
///   3. It always writes a structured ProblemDetails body so clients parse JSON, not HTML.
///   4. It always logs at Error level so unhandled exceptions are observable in prod.
/// </summary>
public class GlobalExceptionHandlerTests
{
    // ── Handler claims the exception ───────────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_ReturnsTrue_SoNoPipelineHandlerRunsNext()
    {
        // Returning true tells ASP.NET Core that this handler has fully dealt with
        // the exception. If we returned false, the framework would continue looking
        // for more handlers and could produce a duplicate or blank response.
        var (handler, context) = Build();

        var result = await handler.TryHandleAsync(context, new Exception("boom"), CancellationToken.None);

        result.Should().BeTrue();
    }

    // ── Status code ───────────────────────────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_SetsResponseStatusCode_To500()
    {
        // Any unhandled exception is a server-side fault regardless of its type.
        // Clients must never receive a 200 or 4xx for a server bug.
        var (handler, context) = Build();

        await handler.TryHandleAsync(context, new Exception("boom"), CancellationToken.None);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // ── Response body ─────────────────────────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_WritesValidProblemDetailsBody()
    {
        // Clients depend on a structured JSON body to display useful error messages
        // without crashing their own JSON parsers on an unexpected HTML error page.
        var (handler, context) = Build();

        await handler.TryHandleAsync(context, new Exception("boom"), CancellationToken.None);

        var problem = await ReadProblemDetailsAsync(context);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Title.Should().Be("An unexpected error occurred.");
        problem.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.6.1");
    }

    // ── Logging ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_LogsException_AtErrorLevel()
    {
        // Without error-level logging, a silent 500 in production would produce no
        // alert and leave no trace in log aggregators (Datadog, Seq, etc.).
        // The original exception object must be attached so stack traces are captured.
        var logger = new Mock<ILogger<GlobalExceptionHandler>>();
        var context = BuildContext();
        var handler = new GlobalExceptionHandler(logger.Object);
        var exception = new InvalidOperationException("unexpected state");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ── Exception type coverage ────────────────────────────────────────────────

    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(ArgumentNullException))]
    [InlineData(typeof(TimeoutException))]
    [InlineData(typeof(NotImplementedException))]
    public async Task TryHandleAsync_Returns500_ForAnyExceptionType(Type exceptionType)
    {
        // The handler intentionally does not branch on exception type. Every unhandled
        // exception is a server fault: returning different status codes based on type
        // would leak internal implementation details to external callers.
        var (handler, context) = Build();
        var exception = (Exception)Activator.CreateInstance(exceptionType, "test")!;

        var claimed = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        claimed.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (GlobalExceptionHandler handler, DefaultHttpContext context) Build()
    {
        var logger = new Mock<ILogger<GlobalExceptionHandler>>();
        return (new GlobalExceptionHandler(logger.Object), BuildContext());
    }

    private static DefaultHttpContext BuildContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private static async Task<ProblemDetails?> ReadProblemDetailsAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
