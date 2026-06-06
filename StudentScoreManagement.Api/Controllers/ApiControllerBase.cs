using Microsoft.AspNetCore.Mvc;
using StudentScoreManagement.Application.Common.ApiResponse;

namespace StudentScoreManagement.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected int? CurrentStudentId
    {
        get
        {
            var value = User.FindFirst("studentId")?.Value;
            return int.TryParse(value, out var studentId) ? studentId : null;
        }
    }

    protected IActionResult ForbiddenResponse()
    {
        return StatusCode(
            StatusCodes.Status403Forbidden,
            ApiResponse<object?>.Fail("Forbidden", new[] { "You do not have permission to access this resource." }));
    }

    protected IActionResult NotFoundResponse(string message)
    {
        return NotFound(ApiResponse<object?>.Fail("Not found", new[] { message }));
    }
}
