using StudentScoreManagement.Application.DTOs.Scores;
using StudentScoreManagement.Application.DTOs.Students;
using StudentScoreManagement.Application.DTOs.Subjects;
using StudentScoreManagement.Application.DTOs.Users;
using StudentScoreManagement.Domain.Entities;

namespace StudentScoreManagement.Infrastructure.Services;

internal static class MappingExtensions
{
    public static StudentResponseDto ToResponseDto(this Student student)
    {
        return new StudentResponseDto
        {
            Id = student.Id,
            StudentCode = student.StudentCode,
            FullName = student.FullName,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender.ToString(),
            ClassName = student.ClassName,
            Email = student.Email,
            Phone = student.Phone,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };
    }

    public static SubjectResponseDto ToResponseDto(this Subject subject)
    {
        return new SubjectResponseDto
        {
            Id = subject.Id,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credit = subject.Credit,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt
        };
    }

    public static ScoreResponseDto ToResponseDto(this Score score)
    {
        return new ScoreResponseDto
        {
            Id = score.Id,
            StudentId = score.StudentId,
            StudentCode = score.Student.StudentCode,
            StudentName = score.Student.FullName,
            ClassName = score.Student.ClassName,
            SubjectId = score.SubjectId,
            SubjectCode = score.Subject.SubjectCode,
            SubjectName = score.Subject.SubjectName,
            ScoreValue = score.ScoreValue,
            Semester = score.Semester,
            SchoolYear = score.SchoolYear,
            CreatedAt = score.CreatedAt,
            UpdatedAt = score.UpdatedAt
        };
    }

    public static AppUserResponseDto ToResponseDto(this AppUser user)
    {
        return new AppUserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            StudentId = user.StudentId,
            CreatedAt = user.CreatedAt
        };
    }
}
