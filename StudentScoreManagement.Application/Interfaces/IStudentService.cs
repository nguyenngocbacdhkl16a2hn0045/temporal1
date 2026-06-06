using StudentScoreManagement.Application.Common.Filtering;
using StudentScoreManagement.Application.Common.Pagination;
using StudentScoreManagement.Application.DTOs.Students;

namespace StudentScoreManagement.Application.Interfaces;

public interface IStudentService
{
    Task<PagedResult<StudentResponseDto>> GetStudentsAsync(StudentQueryParameters query, CancellationToken cancellationToken = default);
    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StudentResponseDto> CreateAsync(StudentCreateDto dto, CancellationToken cancellationToken = default);
    Task<StudentResponseDto?> UpdateAsync(int id, StudentUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
