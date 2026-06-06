using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentScoreManagement.Application.Common.ApiResponse;
using StudentScoreManagement.Application.DTOs.Subjects;
using StudentScoreManagement.Application.Interfaces;

namespace StudentScoreManagement.Api.Controllers;

[Route("api/subjects")]
[Authorize]
public class SubjectsController : ApiControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpGet]
    public async Task<IActionResult> GetSubjects(CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetSubjectsAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(result, "Get subjects successful"));
    }

    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Subject not found.");
        }

        return Ok(ApiResponse<SubjectResponseDto>.Ok(result, "Get subject successful"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(SubjectCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.CreateAsync(dto, cancellationToken);
        return Ok(ApiResponse<SubjectResponseDto>.Ok(result, "Create subject successful"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SubjectUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.UpdateAsync(id, dto, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Subject not found.");
        }

        return Ok(ApiResponse<SubjectResponseDto>.Ok(result, "Update subject successful"));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _subjectService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFoundResponse("Subject not found.");
        }

        return Ok(ApiResponse<object?>.Ok(null, "Delete subject successful"));
    }
}
