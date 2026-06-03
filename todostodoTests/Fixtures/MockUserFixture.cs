using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using todostodo.Models;

namespace todostodo.Tests.Fixtures;

/// <summary>
/// Fixture for creating mock test users and user managers.
/// Provides consistent test user data across tests.
/// </summary>
public class MockUserFixture
{
    /// <summary>
    /// Creates a test user with default credentials.
    /// </summary>
    public static ApplicationUser CreateTestUser(int? userId = null)
    {
        return new ApplicationUser
        {
            Id = userId ?? 1,
            UserName = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    /// Creates a mock UserManager for testing.
    /// </summary>
    public static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    /// <summary>
    /// Creates a mock SignInManager for testing.
    /// </summary>
    public static Mock<SignInManager<ApplicationUser>> CreateMockSignInManager()
    {
        var userManagerMock = CreateMockUserManager();
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        
        return new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null!, null!, null!, null!);
    }

    /// <summary>
    /// Creates a ClaimsPrincipal for a test user.
    /// </summary>
    public static ClaimsPrincipal CreateClaimsPrincipal(int userId = 1, string username = "testuser")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Creates a LoginModel for testing.
    /// </summary>
    public static LoginModel CreateLoginModel(string username = "testuser", string password = "TestPassword123!")
    {
        return new LoginModel
        {
            Username = username,
            Password = password
        };
    }

    /// <summary>
    /// Creates a RegisterModel for testing.
    /// </summary>
    public static RegisterModel CreateRegisterModel(
        string username = "newuser", 
        string email = "newuser@example.com", 
        string password = "TestPassword123!")
    {
        return new RegisterModel
        {
            Username = username,
            Email = email,
            Password = password
        };
    }
}
