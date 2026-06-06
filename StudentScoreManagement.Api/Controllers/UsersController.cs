using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentScoreManagement.Application.Common.ApiResponse;
using StudentScoreManagement.Application.DTOs.Users;
using StudentScoreManagement.Application.Interfaces;

namespace StudentScoreManagement.Api.Controllers;

[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _userService.GetUsersAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(result, "Get users successful"));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("User not found.");
        }

        return Ok(ApiResponse<AppUserResponseDto>.Ok(result, "Get user successful"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _userService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFoundResponse("User not found.");
        }

        return Ok(ApiResponse<object?>.Ok(null, "Delete user successful"));
    }
}
