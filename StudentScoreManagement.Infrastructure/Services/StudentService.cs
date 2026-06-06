using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentScoreManagement.Application.Common.Filtering;
using StudentScoreManagement.Application.Common.Pagination;
using StudentScoreManagement.Application.Common.Sorting;
using StudentScoreManagement.Application.DTOs.Students;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Domain.Entities;
using StudentScoreManagement.Domain.Enums;
using StudentScoreManagement.Infrastructure.Data;

namespace StudentScoreManagement.Infrastructure.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StudentService> _logger;

    public StudentService(AppDbContext dbContext, ILogger<StudentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PagedResult<StudentResponseDto>> GetStudentsAsync(StudentQueryParameters query, CancellationToken cancellationToken = default)
    {
        query.Normalize();

        var students = _dbContext.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            students = students.Where(x =>
                x.StudentCode.Contains(keyword) ||
                x.FullName.Contains(keyword) ||
                x.Email.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.ClassName))
        {
            var className = query.ClassName.Trim();
            students = students.Where(x => x.ClassName == className);
        }

        students = ApplySorting(students, query.SortBy, query.SortOrder);

        var totalItems = await students.CountAsync(cancellationToken);
        var studentItems = await students
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = studentItems.Select(x => x.ToResponseDto()).ToList();

        return new PagedResult<StudentResponseDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize)
        };
    }

    public async Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return student?.ToResponseDto();
    }

    public async Task<StudentResponseDto> CreateAsync(StudentCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Students.AnyAsync(x => x.StudentCode == dto.StudentCode, cancellationToken))
        {
            throw new InvalidOperationException("StudentCode already exists.");
        }

        var student = new Student
        {
            StudentCode = dto.StudentCode.Trim(),
            FullName = dto.FullName.Trim(),
            DateOfBirth = dto.DateOfBirth,
            Gender = ParseGender(dto.Gender),
            ClassName = dto.ClassName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Students.AddAsync(student, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created student {StudentCode}", student.StudentCode);
        return student.ToResponseDto();
    }

    public async Task<StudentResponseDto?> UpdateAsync(int id, StudentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (student is null)
        {
            return null;
        }

        student.FullName = dto.FullName.Trim();
        student.DateOfBirth = dto.DateOfBirth;
        student.Gender = ParseGender(dto.Gender);
        student.ClassName = dto.ClassName.Trim();
        student.Email = dto.Email.Trim();
        student.Phone = dto.Phone.Trim();
        student.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated student {StudentCode}", student.StudentCode);

        return student.ToResponseDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (student is null)
        {
            return false;
        }

        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted student {StudentCode}", student.StudentCode);

        return true;
    }

    private static IQueryable<Student> ApplySorting(IQueryable<Student> query, string? sortBy, string? sortOrder)
    {
        var descending = SortDirection.IsDescending(sortOrder);
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "studentcode" => descending ? query.OrderByDescending(x => x.StudentCode) : query.OrderBy(x => x.StudentCode),
            "fullname" or "name" => descending ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "classname" => descending ? query.OrderByDescending(x => x.ClassName) : query.OrderBy(x => x.ClassName),
            "createdat" => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Id)
        };
    }

    private static Gender ParseGender(string? value)
    {
        if (Enum.TryParse<Gender>(value, true, out var gender))
        {
            return gender;
        }

        return value?.Trim().ToLowerInvariant() switch
        {
            "nam" => Gender.Male,
            "nu" or "nữ" => Gender.Female,
            _ => Gender.Unknown
        };
    }
}
