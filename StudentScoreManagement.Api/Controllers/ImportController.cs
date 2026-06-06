using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentScoreManagement.Api.Models;
using StudentScoreManagement.Application.Common.ApiResponse;
using StudentScoreManagement.Application.DTOs.Import;
using StudentScoreManagement.Application.Interfaces;

namespace StudentScoreManagement.Api.Controllers;

[Route("api/import")]
[Authorize(Roles = "Admin")]
public class ImportController : ApiControllerBase
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("scores-csv")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportScoresCsv([FromForm] ImportScoresCsvRequest request, CancellationToken cancellationToken)
    {
        if (request.File.Length == 0)
        {
            return BadRequest(ApiResponse<object?>.Fail("Import failed", new[] { "CSV file is required." }));
        }

        await using var stream = request.File.OpenReadStream();
        var result = await _importService.ImportScoresCsvAsync(stream, cancellationToken);

        return Ok(ApiResponse<ImportResultDto>.Ok(result, "Import CSV successful"));
    }
}
