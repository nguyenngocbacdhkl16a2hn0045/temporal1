using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentScoreManagement.Application.DTOs.Subjects;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Domain.Entities;
using StudentScoreManagement.Infrastructure.Data;

namespace StudentScoreManagement.Infrastructure.Services;

public class SubjectService : ISubjectService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(AppDbContext dbContext, ILogger<SubjectService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SubjectResponseDto>> GetSubjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects
            .AsNoTracking()
            .OrderBy(x => x.SubjectCode)
            .Select(x => new SubjectResponseDto
            {
                Id = x.Id,
                SubjectCode = x.SubjectCode,
                SubjectName = x.SubjectName,
                Credit = x.Credit,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SubjectResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var subject = await _dbContext.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return subject?.ToResponseDto();
    }

    public async Task<SubjectResponseDto> CreateAsync(SubjectCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Subjects.AnyAsync(x => x.SubjectCode == dto.SubjectCode, cancellationToken))
        {
            throw new InvalidOperationException("SubjectCode already exists.");
        }

        var subject = new Subject
        {
            SubjectCode = dto.SubjectCode.Trim(),
            SubjectName = dto.SubjectName.Trim(),
            Credit = dto.Credit,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Subjects.AddAsync(subject, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created subject {SubjectCode}", subject.SubjectCode);
        return subject.ToResponseDto();
    }

    public async Task<SubjectResponseDto?> UpdateAsync(int id, SubjectUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var subject = await _dbContext.Subjects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (subject is null)
        {
            return null;
        }

        subject.SubjectName = dto.SubjectName.Trim();
        subject.Credit = dto.Credit;
        subject.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated subject {SubjectCode}", subject.SubjectCode);

        return subject.ToResponseDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var subject = await _dbContext.Subjects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (subject is null)
        {
            return false;
        }

        _dbContext.Subjects.Remove(subject);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted subject {SubjectCode}", subject.SubjectCode);

        return true;
    }
}
