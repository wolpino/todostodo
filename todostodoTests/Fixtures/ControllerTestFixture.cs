using Microsoft.AspNetCore.Mvc;
using todostodo.Controllers;
using TodoToDo = todostodo.Models.ToDo;

namespace todostodo.Tests.Fixtures;

/// <summary>
/// Base fixture for controller tests.
/// Provides common setup and helper methods for testing controllers.
/// </summary>
public class ControllerTestFixture
{
    /// <summary>
    /// Sets up a controller with a mocked user context.
    /// </summary>
    public static void SetControllerUser<T>(T controller, int userId = 1) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = MockUserFixture.CreateClaimsPrincipal(userId)
            }
        };
    }

    /// <summary>
    /// Helper to extract error messages from ModelState.
    /// </summary>
    public static List<string> GetModelStateErrors(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        var errors = new List<string>();
        foreach (var modelValue in modelState.Values)
        {
            foreach (var error in modelValue.Errors)
            {
                errors.Add(error.ErrorMessage);
            }
        }
        return errors;
    }

    /// <summary>
    /// Helper to check if a response is an error response.
    /// </summary>
    public static bool IsErrorResponse(IActionResult result)
    {
        return result is BadRequestObjectResult or NotFoundResult or UnauthorizedResult;
    }
}
