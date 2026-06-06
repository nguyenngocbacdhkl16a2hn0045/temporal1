using StudentScoreManagement.Application.DTOs.Import;

namespace StudentScoreManagement.Application.Interfaces;

public interface IImportService
{
    Task<ImportResultDto> ImportScoresCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
}
