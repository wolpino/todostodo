using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace todostodo.api.test.Integration;

/// <summary>
/// End-to-end tests for the ASP.NET Core Identity authentication pipeline.
///
/// Unit tests call controller methods directly and bypass every piece of middleware,
/// so they cannot verify that <c>[Authorize]</c>, cookie/bearer token issuance, or
/// password validation actually work. These tests send real HTTP requests through the
/// full pipeline — including authentication middleware — to confirm the wiring is correct.
///
/// Test isolation: each test in this class gets its own <see cref="HttpClient"/> (because
/// xUnit instantiates the test class once per test method). However, the
/// <see cref="CustomWebApplicationFactory"/> and its SQLite database are shared across
/// all tests in the class. Use unique email addresses per test to avoid conflicts.
/// </summary>
public class AuthFlowIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    // ── Registration ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_Returns200_WithValidCredentials()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/register",
            new { email = "register_ok@example.com", password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_Returns400_WhenPasswordIsTooWeak()
    {
        // ASP.NET Core Identity enforces password strength by default (uppercase,
        // lowercase, digit, non-alphanumeric). Weak passwords are a security risk
        // and must be rejected before the account is created.
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/register",
            new { email = "weak_password@example.com", password = "abc" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_Returns400_WhenEmailIsAlreadyTaken()
    {
        // Duplicate registration must be rejected to keep each identity unique.
        // Allowing duplicates would let attackers lock out a legitimate user by
        // registering first with a known email.
        var client = factory.CreateClient();
        const string email = "duplicate_register@example.com";
        await client.PostAsJsonAsync("/register", new { email, password = "Password1!" });

        var second = await client.PostAsJsonAsync("/register",
            new { email, password = "Password1!" });

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_Returns200_AndReturnsAccessToken_WithValidCredentials()
    {
        // The access token is what the SPA attaches to every subsequent API request.
        // This test verifies the full issuance path works end-to-end.
        var client = factory.CreateClient();
        const string email = "login_ok@example.com";
        await client.PostAsJsonAsync("/register", new { email, password = "Password1!" });

        var loginResponse = await client.PostAsJsonAsync("/login",
            new { email, password = "Password1!" });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Returns401_WithWrongPassword()
    {
        // A wrong password must never yield a token. Returning a non-401 here would be
        // a critical authentication bypass.
        var client = factory.CreateClient();
        const string email = "login_wrongpass@example.com";
        await client.PostAsJsonAsync("/register", new { email, password = "Password1!" });

        var loginResponse = await client.PostAsJsonAsync("/login",
            new { email, password = "WrongPassword999!" });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Returns401_ForUnregisteredEmail()
    {
        // Non-existent accounts must not produce a token. The response should be identical
        // to a wrong-password failure to avoid user enumeration via timing or error messages.
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/login",
            new { email = "ghost_user@example.com", password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Authenticated entry creation ───────────────────────────────────────────

    [Fact]
    public async Task CreateEntry_Returns201_WhenCalledWithValidBearerToken()
    {
        // Full roundtrip: register → login → create entry.
        // This validates the entire auth pipeline, including token validation on the
        // [Authorize] route, which unit tests cannot exercise.
        var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client, "create_auth@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/Entry",
            new { title = "Integration Test Entry", status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateEntry_Returns401_WhenCalledWithoutToken()
    {
        // This is the most critical auth test: it confirms that the [Authorize] attribute
        // on the Create action is actually enforced by the middleware pipeline.
        // A unit test calling the controller method directly would always succeed here
        // because it bypasses the auth middleware entirely.
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Entry",
            new { title = "Unauthorized Entry", status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_Returns200_WhenAuthenticated()
    {
        var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client, "logout_ok@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_Returns401_WhenNotAuthenticated()
    {
        // /logout is mapped with RequireAuthorization(). An unauthenticated caller must
        // receive 401 — not 200, not 404. This test uses a fresh client with no auth
        // header to simulate a missing or expired token.
        var unauthClient = factory.CreateClient();

        var response = await unauthClient.PostAsync("/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
