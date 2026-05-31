using todostodo.Models;
using Microsoft.AspNetCore.Authentication;

namespace todostodo;

public static class HttpContextExtensions
{
    public static async Task<AuthenticationScheme[]> GetExternalProvidersAsync(this HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

        return (from scheme in await schemes.GetAllSchemesAsync()
            where !string.IsNullOrEmpty(scheme.DisplayName)
            select scheme).ToArray();
    }

    public static async Task<bool> IsProviderSupportedAsync(this HttpContext context, string provider)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return (from scheme in await context.GetExternalProvidersAsync()
            where string.Equals(scheme.Name, provider)
            select scheme).Any();
    }
    /// <summary>
    /// Gets the currently authenticated user from HttpContext items.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The authenticated ApplicationUser or null if not found.</returns>
    public static ApplicationUser? GetUser(this HttpContext context) 
        => context?.Items["User"] as ApplicationUser;
}