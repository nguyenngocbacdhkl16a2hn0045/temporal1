using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentScoreManagement.Application.Common.ApiResponse;
using StudentScoreManagement.Application.Common.Filtering;
using StudentScoreManagement.Application.DTOs.Scores;
using StudentScoreManagement.Application.Interfaces;

namespace StudentScoreManagement.Api.Controllers;

[Route("api/scores")]
[Authorize]
public class ScoresController : ApiControllerBase
{
    private readonly IScoreService _scoreService;

    public ScoresController(IScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpGet]
    public async Task<IActionResult> GetScores([FromQuery] ScoreQueryParameters query, CancellationToken cancellationToken)
    {
        var onlyStudentId = User.IsInRole("Student") ? CurrentStudentId : null;
        if (User.IsInRole("Student") && onlyStudentId is null)
        {
            return ForbiddenResponse();
        }

        var result = await _scoreService.GetScoresAsync(query, onlyStudentId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result, "Get scores successful"));
    }

    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _scoreService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Score not found.");
        }

        if (User.IsInRole("Student") && CurrentStudentId != result.StudentId)
        {
            return ForbiddenResponse();
        }

        return Ok(ApiResponse<ScoreResponseDto>.Ok(result, "Get score successful"));
    }

    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpGet("student/{studentId:int}")]
    public async Task<IActionResult> GetByStudentId(int studentId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Student") && CurrentStudentId != studentId)
        {
            return ForbiddenResponse();
        }

        var result = await _scoreService.GetByStudentIdAsync(studentId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result, "Get student scores successful"));
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpPost]
    public async Task<IActionResult> Create(ScoreCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _scoreService.CreateAsync(dto, cancellationToken);
        return Ok(ApiResponse<ScoreResponseDto>.Ok(result, "Create score successful"));
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ScoreUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _scoreService.UpdateAsync(id, dto, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Score not found.");
        }

        return Ok(ApiResponse<ScoreResponseDto>.Ok(result, "Update score successful"));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _scoreService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFoundResponse("Score not found.");
        }

        return Ok(ApiResponse<object?>.Ok(null, "Delete score successful"));
    }
}
