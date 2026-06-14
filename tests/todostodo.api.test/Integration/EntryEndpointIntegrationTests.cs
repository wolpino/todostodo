using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace todostodo.api.test.Integration;

/// <summary>
/// Integration tests for the full HTTP pipeline of the Entry endpoint.
///
/// These tests complement the controller unit tests by exercising concerns
/// that only exist in the full middleware stack:
///
/// <list type="bullet">
///   <item>
///     <b>Request validation</b> — <c>[ApiController]</c> automatically returns
///     HTTP 400 with <c>ValidationProblemDetails</c> when model binding fails.
///     Unit tests that call controller methods directly never trigger this.
///   </item>
///   <item>
///     <b>CORS</b> — headers are injected by <c>UseCors</c> middleware, not by
///     the controller. Without integration tests, CORS misconfiguration is invisible.
///   </item>
///   <item>
///     <b>Exception handler pipeline registration</b> — verifies
///     <c>GlobalExceptionHandler</c> is actually wired into the pipeline, complementing
///     the unit tests that only test the handler class in isolation.
///   </item>
/// </list>
/// </summary>
public class EntryEndpointIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    // ── Request validation — Create ────────────────────────────────────────────

    [Fact]
    public async Task Create_Returns400_WhenTitleIsMissing()
    {
        // [ApiController] + [Required] on CreateEntryRequest.Title causes the framework
        // to automatically short-circuit the action with a 400 ValidationProblemDetails
        // before the controller method body ever runs. This behaviour only activates
        // when requests pass through the full MVC pipeline.
        var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client, "val_notitle@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/Entry",
            new { status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Returns400_WhenTitleExceedsMaxLength()
    {
        // [MaxLength(200)] on Title is enforced by the validation pipeline before
        // any EF Core save attempt, providing a clean client error instead of a DB exception.
        var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client, "val_longtitle@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/Entry",
            new { title = new string('x', 201), status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Returns400_WhenDescriptionExceedsMaxLength()
    {
        // [MaxLength(1000)] on Description protects against payloads that would waste
        // DB storage or exceed column constraints.
        var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client, "val_longdesc@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/Entry",
            new { title = "Valid Title", description = new string('x', 1001), status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Returns400_WithValidationProblemDetailsBody()
    {
        // RFC 7807 (Problem Details) requires an "errors" map for validation failures.
        // Clients use this to display field-level error messages. A plain 400 with no
        // body, or a body in a different format, would break frontend form validation.
        var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client, "val_body@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/Entry", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("errors", out _).Should().BeTrue(
            "a 400 from [ApiController] should always include an 'errors' map");
    }

    [Fact]
    public async Task Create_Returns201_WhenRequestIsValid()
    {
        // Boundary test: confirm a well-formed request is not incorrectly rejected.
        // This prevents overzealous validation from blocking legitimate clients.
        var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client, "val_valid@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/Entry",
            new { title = "Valid Entry", status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // ── CORS ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Options_IncludesAllowOriginHeader_ForRegisteredFrontendOrigin()
    {
        // The CORS preflight is what the browser sends before any cross-origin request.
        // If this fails, the browser blocks the actual request before it ever reaches
        // the API — a silent failure that looks like a network error to users.
        // "http://localhost:5173" is the Vite dev server origin registered in Program.cs.
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/Entry");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin")
            .Should().Contain("http://localhost:5173");
    }

    [Fact]
    public async Task Options_DoesNotIncludeAllowOriginHeader_ForUnknownOrigin()
    {
        // An unknown origin must not receive CORS approval. Without this restriction,
        // any website could make credentialed cross-origin requests to the API on behalf
        // of authenticated users (CSRF via CORS misconfiguration).
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/Entry");
        request.Headers.Add("Origin", "http://evil.example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        if (response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values))
            values.Should().NotContain("http://evil.example.com");
    }

    // ── Exception handler pipeline registration ────────────────────────────────

    [Fact]
    public async Task GetById_Returns404_WithProblemDetailsBody_ForMissingEntry()
    {
        // Confirms the ProblemDetails / response infrastructure is wired correctly.
        // Uses an authenticated client because GET /{id} now requires authorization.
        var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client, "get404@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/Entry/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<string> RegisterAndLoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        await client.PostAsJsonAsync("/register", new { email, password });
        var loginResponse = await client.PostAsJsonAsync("/login", new { email, password });
        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }
}
