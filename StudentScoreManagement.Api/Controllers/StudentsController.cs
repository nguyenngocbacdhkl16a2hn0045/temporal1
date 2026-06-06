using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentScoreManagement.Application.Common.ApiResponse;
using StudentScoreManagement.Application.Common.Filtering;
using StudentScoreManagement.Application.DTOs.Students;
using StudentScoreManagement.Application.Interfaces;

namespace StudentScoreManagement.Api.Controllers;

[Route("api/students")]
[Authorize]
public class StudentsController : ApiControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] StudentQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _studentService.GetStudentsAsync(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result, "Get students successful"));
    }

    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Student") && CurrentStudentId != id)
        {
            return ForbiddenResponse();
        }

        var result = await _studentService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Student not found.");
        }

        return Ok(ApiResponse<StudentResponseDto>.Ok(result, "Get student successful"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(StudentCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _studentService.CreateAsync(dto, cancellationToken);
        return Ok(ApiResponse<StudentResponseDto>.Ok(result, "Create student successful"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, StudentUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _studentService.UpdateAsync(id, dto, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Student not found.");
        }

        return Ok(ApiResponse<StudentResponseDto>.Ok(result, "Update student successful"));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _studentService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFoundResponse("Student not found.");
        }

        return Ok(ApiResponse<object?>.Ok(null, "Delete student successful"));
    }
}
