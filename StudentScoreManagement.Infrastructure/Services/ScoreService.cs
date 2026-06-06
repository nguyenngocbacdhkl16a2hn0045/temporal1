using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentScoreManagement.Application.Common.Filtering;
using StudentScoreManagement.Application.Common.Pagination;
using StudentScoreManagement.Application.Common.Sorting;
using StudentScoreManagement.Application.DTOs.Scores;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Domain.Entities;
using StudentScoreManagement.Infrastructure.Data;

namespace StudentScoreManagement.Infrastructure.Services;

public class ScoreService : IScoreService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ScoreService> _logger;

    public ScoreService(AppDbContext dbContext, ILogger<ScoreService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PagedResult<ScoreResponseDto>> GetScoresAsync(
        ScoreQueryParameters query,
        int? onlyStudentId = null,
        CancellationToken cancellationToken = default)
    {
        query.Normalize();

        var scores = _dbContext.Scores
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Subject)
            .AsQueryable();

        if (onlyStudentId.HasValue)
        {
            scores = scores.Where(x => x.StudentId == onlyStudentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.StudentCode))
        {
            var studentCode = query.StudentCode.Trim();
            scores = scores.Where(x => x.Student.StudentCode.Contains(studentCode));
        }

        if (!string.IsNullOrWhiteSpace(query.StudentName))
        {
            var studentName = query.StudentName.Trim();
            scores = scores.Where(x => x.Student.FullName.Contains(studentName));
        }

        if (!string.IsNullOrWhiteSpace(query.ClassName))
        {
            var className = query.ClassName.Trim();
            scores = scores.Where(x => x.Student.ClassName == className);
        }

        if (query.SubjectId.HasValue)
        {
            scores = scores.Where(x => x.SubjectId == query.SubjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Semester))
        {
            var semester = query.Semester.Trim();
            scores = scores.Where(x => x.Semester == semester);
        }

        if (!string.IsNullOrWhiteSpace(query.SchoolYear))
        {
            var schoolYear = query.SchoolYear.Trim();
            scores = scores.Where(x => x.SchoolYear == schoolYear);
        }

        if (query.MinScore.HasValue)
        {
            scores = scores.Where(x => x.ScoreValue >= query.MinScore.Value);
        }

        if (query.MaxScore.HasValue)
        {
            scores = scores.Where(x => x.ScoreValue <= query.MaxScore.Value);
        }

        scores = ApplySorting(scores, query.SortBy, query.SortOrder);

        var totalItems = await scores.CountAsync(cancellationToken);
        var items = await scores
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ScoreResponseDto
            {
                Id = x.Id,
                StudentId = x.StudentId,
                StudentCode = x.Student.StudentCode,
                StudentName = x.Student.FullName,
                ClassName = x.Student.ClassName,
                SubjectId = x.SubjectId,
                SubjectCode = x.Subject.SubjectCode,
                SubjectName = x.Subject.SubjectName,
                ScoreValue = x.ScoreValue,
                Semester = x.Semester,
                SchoolYear = x.SchoolYear,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ScoreResponseDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize)
        };
    }

    public async Task<ScoreResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Scores
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Subject)
            .Where(x => x.Id == id)
            .Select(x => new ScoreResponseDto
            {
                Id = x.Id,
                StudentId = x.StudentId,
                StudentCode = x.Student.StudentCode,
                StudentName = x.Student.FullName,
                ClassName = x.Student.ClassName,
                SubjectId = x.SubjectId,
                SubjectCode = x.Subject.SubjectCode,
                SubjectName = x.Subject.SubjectName,
                ScoreValue = x.ScoreValue,
                Semester = x.Semester,
                SchoolYear = x.SchoolYear,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScoreResponseDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Scores
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Subject)
            .Where(x => x.StudentId == studentId)
            .OrderBy(x => x.Subject.SubjectCode)
            .Select(x => new ScoreResponseDto
            {
                Id = x.Id,
                StudentId = x.StudentId,
                StudentCode = x.Student.StudentCode,
                StudentName = x.Student.FullName,
                ClassName = x.Student.ClassName,
                SubjectId = x.SubjectId,
                SubjectCode = x.Subject.SubjectCode,
                SubjectName = x.Subject.SubjectName,
                ScoreValue = x.ScoreValue,
                Semester = x.Semester,
                SchoolYear = x.SchoolYear,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ScoreResponseDto> CreateAsync(ScoreCreateDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureStudentAndSubjectExistAsync(dto.StudentId, dto.SubjectId, cancellationToken);

        var existingScore = await _dbContext.Scores.FirstOrDefaultAsync(x =>
            x.StudentId == dto.StudentId &&
            x.SubjectId == dto.SubjectId &&
            x.Semester == dto.Semester &&
            x.SchoolYear == dto.SchoolYear,
            cancellationToken);

        if (existingScore is not null)
        {
            throw new InvalidOperationException("Score already exists for this student, subject, semester and school year.");
        }

        var score = new Score
        {
            StudentId = dto.StudentId,
            SubjectId = dto.SubjectId,
            ScoreValue = dto.ScoreValue,
            Semester = dto.Semester.Trim(),
            SchoolYear = dto.SchoolYear.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Scores.AddAsync(score, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created score for StudentId {StudentId}, SubjectId {SubjectId}", score.StudentId, score.SubjectId);
        return (await GetByIdAsync(score.Id, cancellationToken))!;
    }

    public async Task<ScoreResponseDto?> UpdateAsync(int id, ScoreUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var score = await _dbContext.Scores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (score is null)
        {
            return null;
        }

        score.ScoreValue = dto.ScoreValue;
        score.Semester = dto.Semester.Trim();
        score.SchoolYear = dto.SchoolYear.Trim();
        score.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated score {ScoreId}", score.Id);

        return await GetByIdAsync(score.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var score = await _dbContext.Scores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (score is null)
        {
            return false;
        }

        _dbContext.Scores.Remove(score);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted score {ScoreId}", score.Id);

        return true;
    }

    private async Task EnsureStudentAndSubjectExistAsync(int studentId, int subjectId, CancellationToken cancellationToken)
    {
        var studentExists = await _dbContext.Students.AnyAsync(x => x.Id == studentId, cancellationToken);
        if (!studentExists)
        {
            throw new InvalidOperationException("Student does not exist.");
        }

        var subjectExists = await _dbContext.Subjects.AnyAsync(x => x.Id == subjectId, cancellationToken);
        if (!subjectExists)
        {
            throw new InvalidOperationException("Subject does not exist.");
        }
    }

    private static IQueryable<Score> ApplySorting(IQueryable<Score> query, string? sortBy, string? sortOrder)
    {
        var descending = SortDirection.IsDescending(sortOrder);
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "studentcode" => descending ? query.OrderByDescending(x => x.Student.StudentCode) : query.OrderBy(x => x.Student.StudentCode),
            "studentname" => descending ? query.OrderByDescending(x => x.Student.FullName) : query.OrderBy(x => x.Student.FullName),
            "classname" => descending ? query.OrderByDescending(x => x.Student.ClassName) : query.OrderBy(x => x.Student.ClassName),
            "subjectname" => descending ? query.OrderByDescending(x => x.Subject.SubjectName) : query.OrderBy(x => x.Subject.SubjectName),
            "score" or "scorevalue" => descending ? query.OrderByDescending(x => x.ScoreValue) : query.OrderBy(x => x.ScoreValue),
            "semester" => descending ? query.OrderByDescending(x => x.Semester) : query.OrderBy(x => x.Semester),
            "schoolyear" => descending ? query.OrderByDescending(x => x.SchoolYear) : query.OrderBy(x => x.SchoolYear),
            _ => query.OrderBy(x => x.Id)
        };
    }
}
