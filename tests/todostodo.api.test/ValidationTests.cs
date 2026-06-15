using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentAssertions;
using todostodo.api.Models;

namespace todostodo.api.test;

/// <summary>
/// Unit tests for validation constraints on request DTOs.
///
/// <para>
/// <b>Why reflection instead of <c>Validator.TryValidateObject</c>?</b><br/>
/// ASP.NET Core's model binding reads validation attributes from <em>constructor
/// parameters</em> on record types, not from the synthesised properties. This is
/// by design: MVC uses the primary constructor to bind and validate records, so
/// <c>[Required]</c> and <c>[MaxLength]</c> must live on the parameters — exactly
/// where they are defined in <c>EntryRequests.cs</c>.
/// <c>Validator.TryValidateObject</c> reads from properties, so it cannot see
/// those attributes; using it here would give false passes or false failures.
/// </para>
///
/// <para>
/// These tests therefore verify the attribute contract via reflection: the right
/// attributes are present, on the right parameters, with the right values.
/// Actual HTTP-level validation behaviour (that the pipeline returns 400 for
/// invalid payloads) is covered by
/// <c>Integration/EntryEndpointIntegrationTests.cs</c>.
/// </para>
/// </summary>
public class ValidationTests
{
    // ── CreateEntryRequest ─────────────────────────────────────────────────────

    [Fact]
    public void CreateEntryRequest_Title_HasRequiredAttribute()
    {
        // [Required] on Title means the MVC pipeline rejects requests where
        // title is null or empty before the controller action ever runs.
        GetConstructorParam<CreateEntryRequest>("Title")
            .GetCustomAttribute<RequiredAttribute>()
            .Should().NotBeNull("Title is mandatory — an entry without a title is meaningless");
    }

    [Fact]
    public void CreateEntryRequest_Title_HasMaxLength200()
    {
        // 200 is the DB column limit. Enforcing it at the API boundary gives a clean
        // 400 instead of a DB error or silent truncation.
        GetConstructorParam<CreateEntryRequest>("Title")
            .GetCustomAttribute<MaxLengthAttribute>()!
            .Length.Should().Be(200);
    }

    [Fact]
    public void CreateEntryRequest_Status_DefaultsToActive()
    {
        // New entries default to Active so callers don't have to specify status.
        // If the default changes, API consumers that omit the field will be affected.
        var param = GetConstructorParam<CreateEntryRequest>("Status");
        param.HasDefaultValue.Should().BeTrue();
        param.DefaultValue.Should().Be(EntryStatus.Active);
    }

    // ── UpdateEntryRequest ─────────────────────────────────────────────────────

    [Fact]
    public void UpdateEntryRequest_Id_HasRequiredAttribute()
    {
        // Id ties the request body to the route parameter. The controller validates
        // that they match; [Required] ensures Id is always present in the body.
        GetConstructorParam<UpdateEntryRequest>("Id")
            .GetCustomAttribute<RequiredAttribute>()
            .Should().NotBeNull();
    }

    [Fact]
    public void UpdateEntryRequest_Title_HasMaxLength200()
    {
        // The same column constraint as CreateEntryRequest.Title applies on update.
        GetConstructorParam<UpdateEntryRequest>("Title")
            .GetCustomAttribute<MaxLengthAttribute>()!
            .Length.Should().Be(200);
    }

    [Fact]
    public void UpdateEntryRequest_Title_HasNoRequiredAttribute()
    {
        // Null title in UpdateEntryRequest means "do not change the title".
        // Making it required would force callers to repeat the existing title on every update.
        GetConstructorParam<UpdateEntryRequest>("Title")
            .GetCustomAttribute<RequiredAttribute>()
            .Should().BeNull("Title is optional in an update — null means no change");
    }

    [Fact]
    public void UpdateEntryRequest_Status_HasNoRequiredAttribute()
    {
        // Null status means "do not change the status", mirroring the Title convention.
        GetConstructorParam<UpdateEntryRequest>("Status")
            .GetCustomAttribute<RequiredAttribute>()
            .Should().BeNull("Status is optional in an update — null means no change");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the constructor <see cref="ParameterInfo"/> for the named parameter on
    /// the primary constructor of <typeparamref name="T"/>.
    /// ASP.NET Core MVC reads validation attributes from constructor parameters on
    /// record types, so this is the correct reflection target for these tests.
    /// </summary>
    private static ParameterInfo GetConstructorParam<T>(string name)
    {
        var ctor = typeof(T).GetConstructors().First();
        return ctor.GetParameters().First(p =>
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
