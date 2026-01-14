using Microsoft.AspNetCore.Mvc;
using ECFRApp_test.Models;
using ECFRApp_test.Services;

namespace ECFRApp_test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ECFRDataController : ControllerBase
{
    private readonly IECFRDataService _ecfrDataService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ECFRDataController> _logger;

    public ECFRDataController(
        IECFRDataService ecfrDataService,
        IFileStorageService fileStorageService,
        ILogger<ECFRDataController> logger)
    {
        _ecfrDataService = ecfrDataService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpGet("raw")]
    public async Task<IActionResult> GetRawApiResponse([FromQuery] int pageSize = 10)
    {
        try
        {
            var rawResponse = await _ecfrDataService.GetRawApiResponseAsync(pageSize);
            return Content(rawResponse, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching raw ECFR data");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("fetch")]
    public async Task<IActionResult> FetchAndSaveData([FromQuery] int pageSize = 100)
    {
        try
        {
            _logger.LogInformation("Fetching ECFR data with page size: {PageSize}", pageSize);
            
            var searchResult = await _ecfrDataService.FetchECFRDataAsync(pageSize);
            
            var snapshot = new ECFRDataSnapshot
            {
                Documents = searchResult.Results,
                FetchedAt = DateTime.UtcNow
            };

            await _fileStorageService.SaveDataAsync(snapshot);

            return Ok(new 
            { 
                success = true, 
                message = $"Successfully fetched and saved {searchResult.Results.Count} documents",
                totalResults = searchResult.TotalResults,
                fetchedAt = snapshot.FetchedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching and saving ECFR data");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var statistics = await _fileStorageService.GetAgencyStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("historical")]
    public async Task<IActionResult> GetHistoricalChanges()
    {
        try
        {
            var historicalData = await _fileStorageService.GetHistoricalChangesAsync();
            return Ok(historicalData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving historical data");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("cleanup-historical")]
    public async Task<IActionResult> CleanupHistoricalData()
    {
        try
        {
            await _fileStorageService.CleanupHistoricalDataAsync();
            return Ok(new 
            { 
                success = true, 
                message = "Historical data cleaned up successfully. Invalid entries removed."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up historical data");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("snapshot")]
    public async Task<IActionResult> GetCurrentSnapshot()
    {
        try
        {
            var snapshot = await _fileStorageService.LoadDataAsync();
            if (snapshot == null)
            {
                return NotFound(new { success = false, message = "No data available. Please fetch data first." });
            }
            return Ok(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snapshot");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}