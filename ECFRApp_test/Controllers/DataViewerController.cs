using Microsoft.AspNetCore.Mvc;
using ECFRApp_test.Services;

namespace ECFRApp_test.Controllers;

public class DataViewerController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DataViewerController> _logger;

    public DataViewerController(IFileStorageService fileStorageService, ILogger<DataViewerController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var snapshot = await _fileStorageService.LoadDataAsync();
            return View(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data viewer");
            ViewBag.Error = ex.Message;
            return View(null);
        }
    }

    public async Task<IActionResult> Documents()
    {
        try
        {
            var snapshot = await _fileStorageService.LoadDataAsync();
            return View(snapshot?.Documents ?? new List<ECFRApp_test.Models.ECFRDocument>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documents");
            ViewBag.Error = ex.Message;
            return View(new List<ECFRApp_test.Models.ECFRDocument>());
        }
    }
}