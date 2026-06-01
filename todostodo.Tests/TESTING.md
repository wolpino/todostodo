# xUnit Testing Scaffolding

This document describes the xUnit testing scaffolding set up for the todostodo application following Microsoft best practices.

## Overview

The testing infrastructure is organized into two main categories:

### 1. **Fixtures** (`Fixtures/` folder)
Fixtures provide reusable test data and setup/teardown logic for tests.

#### DatabaseFixture
- **Purpose**: Manages the in-memory SQLite database for tests
- **Implementation**: Implements `IAsyncLifetime` for proper async initialization/cleanup
- **Usage**: Inject into test classes that need database access

```csharp
public class MyTests : IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    
    public MyTests()
    {
        _dbFixture = new DatabaseFixture();
    }
    
    public async Task InitializeAsync() => await _dbFixture.InitializeAsync();
    public async Task DisposeAsync() => await _dbFixture.DisposeAsync();
}
```

#### MockUserFixture
- **Purpose**: Creates mock users and authentication components
- **Key Methods**:
  - `CreateTestUser()` - Creates a test ApplicationUser
  - `CreateMockUserManager()` - Returns a Mock<UserManager<>>
  - `CreateMockSignInManager()` - Returns a Mock<SignInManager<>>
  - `CreateClaimsPrincipal()` - Creates a ClaimsPrincipal for authentication testing
  - `CreateLoginModel()` - Creates a LoginModel for login tests
  - `CreateRegisterModel()` - Creates a RegisterModel for registration tests

#### ToDoFixture
- **Purpose**: Creates test ToDo objects and request models
- **Key Methods**:
  - `CreateTestToDo()` - Creates a ToDo with configurable properties
  - `CreateToDoRequest()` - Creates a CreateToDoRequest
  - `CreateUpdateToDoRequest()` - Creates an UpdateToDoRequest
  - `CreateTestToDos()` - Creates multiple ToDos for bulk testing

#### ControllerTestFixture
- **Purpose**: Helper methods for controller testing
- **Key Methods**:
  - `SetControllerUser()` - Injects a mocked user context into a controller
  - `GetModelStateErrors()` - Extracts error messages from ModelState
  - `IsErrorResponse()` - Checks if a result is an error response

### 2. **Test Classes** (`Controllers/` folder)

#### AuthControllerHappyPathTests
Tests the AuthController for successful authentication scenarios:

- **Login_WithValidCredentials_ReturnsOkWithJwtToken**: Verifies successful login returns a valid JWT token
- **Login_WithValidEmail_ReturnsOkWithJwtToken**: Verifies login works with email instead of username
- **Register_WithValidNewUser_ReturnsOkWithSuccessMessage**: Verifies successful user registration
- **Register_CreatesUserWithCorrectProperties**: Verifies user is created with correct data

#### ToDosControllerHappyPathTests
Tests the ToDosController for successful CRUD operations:

- **GetToDos_WithNoToDos_ReturnsEmptyList**: Verifies empty list when no ToDos exist
- **GetToDos_WithMultipleToDos_ReturnsAllToDosOrderedByNewestFirst**: Verifies correct ordering
- **GetToDo_WithValidId_ReturnsCorrectToDo**: Verifies single ToDo retrieval
- **CreateToDo_WithValidRequest_CreatesToDoSuccessfully**: Verifies ToDo creation
- **CreateToDo_WithDueDateAndTime_IncludesThemInCreatedToDo**: Verifies optional fields
- **UpdateToDo_WithValidRequest_UpdatesToDoSuccessfully**: Verifies ToDo update
- **UpdateToDo_WithStatusChange_UpdatesStatusSuccessfully**: Verifies status updates
- **UpdateToDo_WithPartialData_UpdatesOnlyProvidedFields**: Verifies partial updates
- **DeleteToDo_WithValidId_DeletesToDoSuccessfully**: Verifies ToDo deletion
- **GetToDo_WithInvalidId_ReturnsNotFound**: Verifies 404 for invalid ID
- **GetToDo_WithToDoFromDifferentUser_ReturnsNotFound**: Verifies authorization enforcement
- **DeleteToDo_WithInvalidId_ReturnsNotFound**: Verifies 404 on delete

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run specific test class
```bash
dotnet test --filter "ClassName=todostodo.Tests.Controllers.ToDosControllerHappyPathTests"
```

### Run specific test method
```bash
dotnet test --filter "Name~CreateToDo_WithValidRequest"
```

### Run with coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

### Run with detailed output
```bash
dotnet test -v d
```

## Best Practices Applied

1. **Arrange-Act-Assert Pattern**: All tests follow AAA pattern for clarity
2. **Async Support**: Tests use async/await with proper ToDo handling
3. **IAsyncLifetime**: Database fixtures properly manage async initialization
4. **Fluent Assertions**: Uses FluentAssertions for readable assertions
5. **Mocking**: Moq library for creating mock dependencies
6. **Test Isolation**: Each test is independent and doesn't depend on other tests
7. **Meaningful Names**: Test method names describe exactly what is being tested
8. **XML Documentation**: All fixtures and helpers include documentation comments
9. **Happy Path Tests**: Focus on successful scenarios first (future: add negative path tests)

## Extending the Scaffolding

### Adding New Test Classes

1. Create new test class file in `Controllers/` folder
2. Inherit from appropriate fixture classes or implement `IAsyncLifetime`
3. Follow naming convention: `[ControllerName]HappyPathTests.cs`
4. Use existing fixtures for consistency

Example:
```csharp
public class MyControllerHappyPathTests : IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    private readonly MyController _controller;
    
    public MyControllerHappyPathTests()
    {
        _dbFixture = new DatabaseFixture();
        _controller = new MyController(_dbFixture.DbContext);
    }
    
    public async Task InitializeAsync() => await _dbFixture.InitializeAsync();
    public async Task DisposeAsync() => await _dbFixture.DisposeAsync();
    
    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

### Adding New Fixtures

1. Create new fixture class in `Fixtures/` folder
2. Include comprehensive XML documentation
3. Provide static factory methods for creating test data
4. Implement `IAsyncLifetime` if managing resources

## Dependencies

- **xUnit**: Test framework
- **Moq**: Mocking library for dependencies
- **FluentAssertions**: Fluent assertion API
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core testing utilities
- **Microsoft.EntityFrameworkCore.Sqlite**: In-memory database provider
- **Microsoft.NET.Test.SDK**: Test SDK for running tests

## Future Test Coverage

Additional test suites to implement:
- Negative path tests (error scenarios)
- Integration tests with real database
- API endpoint integration tests
- Performance/load tests
- Security/authorization tests
- Validation tests for request models

## References

- [Microsoft Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
