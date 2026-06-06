using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentScoreManagement.Application.DTOs.Import;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Domain.Entities;
using StudentScoreManagement.Domain.Enums;
using StudentScoreManagement.Infrastructure.Data;

namespace StudentScoreManagement.Infrastructure.Services;

public class CsvImportService : IImportService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(AppDbContext dbContext, ILogger<CsvImportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ImportResultDto> ImportScoresCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var result = new ImportResultDto();
        var rows = new List<ImportScoreRow>();

        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new InvalidOperationException("CSV file is empty.");
        }

        var headers = ParseCsvLine(headerLine).Select(NormalizeKey).ToList();
        var rowNumber = 1;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            rowNumber++;

            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            result.TotalRows++;

            try
            {
                var row = BuildRow(headers, ParseCsvLine(line));
                rows.Add(CreateImportRow(row, rowNumber));
            }
            catch (Exception ex)
            {
                result.FailedRows++;
                result.Errors.Add(new ImportErrorDto
                {
                    RowNumber = rowNumber,
                    Message = ex.Message
                });
            }
        }

        if (rows.Count == 0)
        {
            LogImportResult(result);
            return result;
        }

        await UpsertStudentsAsync(rows, cancellationToken);
        await UpsertSubjectsAsync(rows, cancellationToken);
        await UpsertScoresAsync(rows, cancellationToken);

        result.SuccessRows = rows.Count;
        LogImportResult(result);

        return result;
    }

    private ImportScoreRow CreateImportRow(Dictionary<string, string> row, int rowNumber)
    {
        var studentCode = GetValue(row, "MaSV", "StudentCode", "Ma sinh vien", "ID");
        if (string.IsNullOrWhiteSpace(studentCode))
        {
            throw new InvalidOperationException("StudentCode is required.");
        }

        var fullName = GetValue(row, "HoTen", "FullName", "Ho ten", "Ho va ten");
        fullName = string.IsNullOrWhiteSpace(fullName) ? $"Sinh vien {studentCode}" : fullName;

        var subjectCode = GetValue(row, "MaMon", "SubjectCode", "Ma mon");
        var subjectName = GetValue(row, "MonHoc", "SubjectName", "Mon hoc", "TenMon", "Ten mon");
        if (string.IsNullOrWhiteSpace(subjectCode) && string.IsNullOrWhiteSpace(subjectName))
        {
            throw new InvalidOperationException("SubjectCode or SubjectName is required.");
        }

        subjectCode = string.IsNullOrWhiteSpace(subjectCode)
            ? GenerateSubjectCode(subjectName)
            : subjectCode.Trim();

        var semester = GetValue(row, "HocKy", "Semester", "Hoc ky");
        if (string.IsNullOrWhiteSpace(semester))
        {
            throw new InvalidOperationException("Semester is required.");
        }

        var schoolYear = GetValue(row, "NamHoc", "SchoolYear", "Nam hoc");
        if (string.IsNullOrWhiteSpace(schoolYear))
        {
            throw new InvalidOperationException("SchoolYear is required.");
        }

        var scoreText = GetValue(row, "Diem", "Score", "ScoreValue", "Diem tong ket", "Diem thi");
        if (!TryParseScore(scoreText, out var scoreValue) || scoreValue < 0 || scoreValue > 10)
        {
            throw new InvalidOperationException("ScoreValue must be a number from 0 to 10.");
        }

        return new ImportScoreRow(
            RowNumber: rowNumber,
            StudentCode: studentCode.Trim(),
            FullName: fullName.Trim(),
            ClassName: GetValue(row, "Lop", "ClassName", "Ten lop").Trim(),
            Email: GetValue(row, "Email").Trim(),
            Phone: GetValue(row, "Phone", "SoDienThoai", "So dien thoai").Trim(),
            Gender: ParseGender(GetValue(row, "Gender", "GioiTinh", "Gioi tinh")),
            DateOfBirth: TryParseDate(GetValue(row, "DateOfBirth", "NgaySinh", "Ngay sinh")),
            SubjectCode: subjectCode,
            SubjectName: string.IsNullOrWhiteSpace(subjectName) ? subjectCode : subjectName.Trim(),
            Credit: ParseCredit(GetValue(row, "Credit", "SoTinChi", "So tin chi")),
            ScoreValue: scoreValue,
            Semester: semester.Trim(),
            SchoolYear: schoolYear.Trim());
    }

    private async Task UpsertStudentsAsync(List<ImportScoreRow> rows, CancellationToken cancellationToken)
    {
        var studentCodes = rows.Select(x => x.StudentCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var students = await _dbContext.Students
            .Where(x => studentCodes.Contains(x.StudentCode))
            .ToDictionaryAsync(x => x.StudentCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var row in rows)
        {
            if (!students.TryGetValue(row.StudentCode, out var student))
            {
                student = new Student
                {
                    StudentCode = row.StudentCode,
                    FullName = row.FullName,
                    DateOfBirth = row.DateOfBirth,
                    Gender = row.Gender,
                    ClassName = row.ClassName,
                    Email = row.Email,
                    Phone = row.Phone,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Students.AddAsync(student, cancellationToken);
                students[row.StudentCode] = student;
            }
            else
            {
                student.FullName = row.FullName;
                student.ClassName = row.ClassName;
                student.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertSubjectsAsync(List<ImportScoreRow> rows, CancellationToken cancellationToken)
    {
        var subjectCodes = rows.Select(x => x.SubjectCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var subjectNames = rows.Select(x => x.SubjectName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var subjects = await _dbContext.Subjects
            .Where(x => subjectCodes.Contains(x.SubjectCode) || subjectNames.Contains(x.SubjectName))
            .ToListAsync(cancellationToken);

        var subjectsByCode = subjects
            .GroupBy(x => x.SubjectCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var subjectsByName = subjects
            .GroupBy(x => x.SubjectName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (!subjectsByCode.TryGetValue(row.SubjectCode, out var subject)
                && !subjectsByName.TryGetValue(row.SubjectName, out subject))
            {
                subject = new Subject
                {
                    SubjectCode = row.SubjectCode,
                    SubjectName = row.SubjectName,
                    Credit = row.Credit,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Subjects.AddAsync(subject, cancellationToken);
                subjectsByCode[row.SubjectCode] = subject;
                subjectsByName[row.SubjectName] = subject;
            }
            else
            {
                subject.SubjectName = row.SubjectName;
                subject.Credit = row.Credit;
                subject.UpdatedAt = DateTime.UtcNow;
                subjectsByCode[row.SubjectCode] = subject;
                subjectsByName[row.SubjectName] = subject;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertScoresAsync(List<ImportScoreRow> rows, CancellationToken cancellationToken)
    {
        var studentCodes = rows.Select(x => x.StudentCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var subjectCodes = rows.Select(x => x.SubjectCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var students = await _dbContext.Students
            .Where(x => studentCodes.Contains(x.StudentCode))
            .ToDictionaryAsync(x => x.StudentCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var subjectNames = rows.Select(x => x.SubjectName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var subjectList = await _dbContext.Subjects
            .Where(x => subjectCodes.Contains(x.SubjectCode) || subjectNames.Contains(x.SubjectName))
            .ToListAsync(cancellationToken);

        var subjectsByCode = subjectList
            .GroupBy(x => x.SubjectCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var subjectsByName = subjectList
            .GroupBy(x => x.SubjectName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var studentIds = students.Values.Select(x => x.Id).ToList();
        var subjectIds = subjectList.Select(x => x.Id).Distinct().ToList();

        var existingScores = await _dbContext.Scores
            .Where(x => studentIds.Contains(x.StudentId) && subjectIds.Contains(x.SubjectId))
            .ToListAsync(cancellationToken);

        var scores = existingScores.ToDictionary(
            x => CreateScoreKey(x.StudentId, x.SubjectId, x.Semester, x.SchoolYear));

        foreach (var row in rows)
        {
            var student = students[row.StudentCode];
            var subject = subjectsByCode.TryGetValue(row.SubjectCode, out var subjectByCode)
                ? subjectByCode
                : subjectsByName[row.SubjectName];
            var key = CreateScoreKey(student.Id, subject.Id, row.Semester, row.SchoolYear);

            if (scores.TryGetValue(key, out var score))
            {
                score.ScoreValue = row.ScoreValue;
                score.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                score = new Score
                {
                    StudentId = student.Id,
                    SubjectId = subject.Id,
                    ScoreValue = row.ScoreValue,
                    Semester = row.Semester,
                    SchoolYear = row.SchoolYear,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Scores.AddAsync(score, cancellationToken);
                scores[key] = score;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void LogImportResult(ImportResultDto result)
    {
        _logger.LogInformation(
            "Imported CSV scores. TotalRows={TotalRows}, SuccessRows={SuccessRows}, FailedRows={FailedRows}",
            result.TotalRows,
            result.SuccessRows,
            result.FailedRows);
    }

    private static string CreateScoreKey(int studentId, int subjectId, string semester, string schoolYear)
    {
        return $"{studentId}|{subjectId}|{NormalizeKey(semester)}|{NormalizeKey(schoolYear)}";
    }

    private static Dictionary<string, string> BuildRow(IReadOnlyList<string> headers, IReadOnlyList<string> values)
    {
        var row = new Dictionary<string, string>();
        for (var i = 0; i < headers.Count; i++)
        {
            row[headers[i]] = i < values.Count ? values[i].Trim() : string.Empty;
        }

        return row;
    }

    private static string GetValue(Dictionary<string, string> row, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (row.TryGetValue(NormalizeKey(alias), out var value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static bool TryParseScore(string value, out decimal score)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out score)
            || decimal.TryParse(value, NumberStyles.Number, new CultureInfo("vi-VN"), out score);
    }

    private static int ParseCredit(string value)
    {
        return int.TryParse(value, out var credit) && credit > 0 ? credit : 3;
    }

    private static DateTime? TryParseDate(string value)
    {
        if (DateTime.TryParse(value, new CultureInfo("vi-VN"), DateTimeStyles.None, out var date)
            || DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        return null;
    }

    private static Gender ParseGender(string value)
    {
        if (Enum.TryParse<Gender>(value, true, out var gender))
        {
            return gender;
        }

        return NormalizeKey(value) switch
        {
            "nam" => Gender.Male,
            "nu" => Gender.Female,
            _ => Gender.Unknown
        };
    }

    private static string GenerateSubjectCode(string subjectName)
    {
        var normalized = NormalizeKey(subjectName);
        var code = new string(normalized.Where(char.IsLetterOrDigit).Take(12).ToArray()).ToUpperInvariant();
        return string.IsNullOrWhiteSpace(code) ? $"MH{DateTime.UtcNow.Ticks}" : code;
    }

    private static string NormalizeKey(string value)
    {
        var normalized = value
            .Trim()
            .Replace('\u0111', 'd')
            .Replace('\u0110', 'D')
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark || char.IsWhiteSpace(ch) || ch == '_' || ch == '-')
            {
                continue;
            }

            builder.Append(ch);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (ch == ',' && !insideQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        values.Add(current.ToString());
        return values;
    }

    private sealed record ImportScoreRow(
        int RowNumber,
        string StudentCode,
        string FullName,
        string ClassName,
        string Email,
        string Phone,
        Gender Gender,
        DateTime? DateOfBirth,
        string SubjectCode,
        string SubjectName,
        int Credit,
        decimal ScoreValue,
        string Semester,
        string SchoolYear);
}
