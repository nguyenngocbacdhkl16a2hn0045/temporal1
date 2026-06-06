using StudentScoreManagement.Application.DTOs.Subjects;

namespace StudentScoreManagement.Application.Interfaces;

public interface ISubjectService
{
    Task<IReadOnlyList<SubjectResponseDto>> GetSubjectsAsync(CancellationToken cancellationToken = default);
    Task<SubjectResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SubjectResponseDto> CreateAsync(SubjectCreateDto dto, CancellationToken cancellationToken = default);
    Task<SubjectResponseDto?> UpdateAsync(int id, SubjectUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
