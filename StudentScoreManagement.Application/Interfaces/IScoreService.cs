using StudentScoreManagement.Application.Common.Filtering;
using StudentScoreManagement.Application.Common.Pagination;
using StudentScoreManagement.Application.DTOs.Scores;

namespace StudentScoreManagement.Application.Interfaces;

public interface IScoreService
{
    Task<PagedResult<ScoreResponseDto>> GetScoresAsync(ScoreQueryParameters query, int? onlyStudentId = null, CancellationToken cancellationToken = default);
    Task<ScoreResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScoreResponseDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ScoreResponseDto> CreateAsync(ScoreCreateDto dto, CancellationToken cancellationToken = default);
    Task<ScoreResponseDto?> UpdateAsync(int id, ScoreUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
