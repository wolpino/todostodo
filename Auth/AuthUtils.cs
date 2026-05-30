using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using todostodo.Controllers;
using todostodo.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace todostodo.Auth;

public static class AuthUtils
{
    // TODO: figure out the code smell
    // I don't love this system.threading.tasks.task.completedtask but it avoids an unnecessary allocation of a Task object
    public static async System.Threading.Tasks.Task OnTicketReceived(TicketReceivedContext ctx)
    {
        var userManager = ctx.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
        if (userManager == null)
            throw new InvalidOperationException("UserManager is not registered in the dependency injection container.");

        if (ctx.Principal == null)
            throw new InvalidOperationException("Principal is null in authentication context.");

        var emailClaim = ctx.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(emailClaim))
            throw new InvalidOperationException("Email claim not found or is empty.");

        var email = emailClaim;
        var name = email.Split('@')[0];

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var userExists = await userManager.FindByNameAsync(name);
            if (userExists != null) name = Guid.NewGuid().ToString();

            user = new()
            {
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = name,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user);
        }
        else
        {
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
            }
        }

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = AuthController.GetToken(authClaims);

        await ctx.Response.WriteAsJsonAsync(new JwtData()
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
        });
        ctx.HandleResponse();
    }
}