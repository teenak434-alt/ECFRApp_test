using Microsoft.AspNetCore.Mvc;
using ECFRApp_test.Services;

namespace ECFRApp_test.Controllers;

public class DashboardController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IFileStorageService fileStorageService, ILogger<DashboardController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var statistics = await _fileStorageService.GetAgencyStatisticsAsync();
            var snapshot = await _fileStorageService.LoadDataAsync();
            
            ViewBag.LastUpdated = snapshot?.FetchedAt;
            ViewBag.TotalDocuments = statistics.Sum(s => s.DocumentCount);
            
            return View(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            ViewBag.Error = "Error loading data. Please try again later.";
            return View(new List<ECFRApp_test.Models.AgencyStatistics>());
        }
    }

    public async Task<IActionResult> Historical()
    {
        try
        {
            var historicalData = await _fileStorageService.GetHistoricalChangesAsync();
            return View(historicalData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading historical data");
            ViewBag.Error = "Error loading historical data. Please try again later.";
            return View(new List<ECFRApp_test.Models.HistoricalChange>());
        }
    }

    public async Task<IActionResult> Checksums()
    {
        try
        {
            var statistics = await _fileStorageService.GetAgencyStatisticsAsync();
            return View(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading checksums");
            ViewBag.Error = "Error loading checksums. Please try again later.";
            return View(new List<ECFRApp_test.Models.AgencyStatistics>());
        }
    }
}