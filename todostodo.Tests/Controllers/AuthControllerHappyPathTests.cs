using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using todostodo.Controllers;
using todostodo.Models;
using todostodo.Tests.Fixtures;
using System.Threading.ToDos;

namespace todostodo.Tests.Controllers;

/// <summary>
/// Happy path tests for the AuthController.
/// Tests successful authentication and registration scenarios.
/// </summary>
public class AuthControllerHappyPathTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IOptions<ApiBehaviorOptions>> _apiBehaviorOptionsMock;
    private readonly AuthController _controller;

    public AuthControllerHappyPathTests()
    {
        _userManagerMock = MockUserFixture.CreateMockUserManager();
        _signInManagerMock = MockUserFixture.CreateMockSignInManager();
        _configurationMock = new Mock<IConfiguration>();
        _apiBehaviorOptionsMock = new Mock<IOptions<ApiBehaviorOptions>>();

        // Setup ApiBehaviorOptions to return BadRequest for invalid model state
        var apiBehaviorOptions = new ApiBehaviorOptions();
        _apiBehaviorOptionsMock.Setup(x => x.Value).Returns(apiBehaviorOptions);

        _controller = new AuthController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _configurationMock.Object,
            _apiBehaviorOptionsMock.Object);
    }

    /// <summary>
    /// Test: User successfully logs in with correct credentials and receives a JWT token.
    /// </summary>
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithJwtToken()
    {
        // Arrange
        var testUser = MockUserFixture.CreateTestUser(userId: 1);
        var loginModel = MockUserFixture.CreateLoginModel(username: "testuser");
        
        _userManagerMock
            .Setup(x => x.FindByNameAsync(testUser.UserName!))
            .ReturnsAsync(testUser);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(testUser, loginModel.Password))
            .ReturnsAsync(true);

        // Setup JWT token generation
        _configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns("ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatNeedsToBeAtLeast32Characters");
        _configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("TodoAppIssuer");
        _configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("TodoAppAudience");
        _configurationMock
            .Setup(x => x["Jwt:DurationInMinutes"])
            .Returns("60");

        // Act
        var result = await _controller.Login(loginModel);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        
        var jwtData = okResult.Value as JwtData;
        jwtData.Should().NotBeNull();
        jwtData!.Token.Should().NotBeNullOrEmpty();
        jwtData.Expiration.Should().BeGreaterThan(DateTime.UtcNow);
    }

    /// <summary>
    /// Test: User can log in with email instead of username.
    /// </summary>
    [Fact]
    public async Task Login_WithValidEmail_ReturnsOkWithJwtToken()
    {
        // Arrange
        var testUser = MockUserFixture.CreateTestUser(userId: 2);
        var loginModel = new LoginModel { Username = "test@example.com", Password = "TestPassword123!" };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(loginModel.Username))
            .ReturnsAsync((ApplicationUser)null!);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(loginModel.Username))
            .ReturnsAsync(testUser);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(testUser, loginModel.Password))
            .ReturnsAsync(true);

        _configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns("ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatNeedsToBeAtLeast32Characters");
        _configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("TodoAppIssuer");
        _configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("TodoAppAudience");
        _configurationMock
            .Setup(x => x["Jwt:DurationInMinutes"])
            .Returns("60");

        // Act
        var result = await _controller.Login(loginModel);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Test: New user successfully registers and receives a success response.
    /// </summary>
    [Fact]
    public async Task Register_WithValidNewUser_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var registerModel = MockUserFixture.CreateRegisterModel(
            username: "newuser",
            email: "newuser@example.com");

        _userManagerMock
            .Setup(x => x.FindByNameAsync(registerModel.Username))
            .ReturnsAsync((ApplicationUser)null!);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(registerModel.Email))
            .ReturnsAsync((ApplicationUser)null!);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerModel.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(registerModel);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as Response;
        response.Should().NotBeNull();
        response!.Status.Should().Be("Success");
        response.Message.Should().NotBeNullOrEmpty();

        // Verify that CreateAsync was called once with the correct parameters
        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerModel.Password),
            Times.Once);
    }

    /// <summary>
    /// Test: Register creates user with correct properties.
    /// </summary>
    [Fact]
    public async Task Register_CreatesUserWithCorrectProperties()
    {
        // Arrange
        var registerModel = MockUserFixture.CreateRegisterModel(
            username: "anotheruser",
            email: "another@example.com");

        _userManagerMock
            .Setup(x => x.FindByNameAsync(registerModel.Username))
            .ReturnsAsync((ApplicationUser)null!);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(registerModel.Email))
            .ReturnsAsync((ApplicationUser)null!);

        ApplicationUser? createdUser = null;
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerModel.Password))
            .Callback<ApplicationUser, string>((user, password) =>
            {
                createdUser = user;
            })
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(registerModel);

        // Assert
        createdUser.Should().NotBeNull();
        createdUser!.UserName.Should().Be(registerModel.Username);
        createdUser.Email.Should().Be(registerModel.Email);
        createdUser.SecurityStamp.Should().NotBeNullOrEmpty();
    }
}
