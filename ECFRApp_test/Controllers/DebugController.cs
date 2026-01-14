using Microsoft.AspNetCore.Mvc;
using ECFRApp_test.Services;

namespace ECFRApp_test.Controllers;

public class DebugController : Controller
{
    private readonly IECFRDataService _ecfrDataService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DebugController> _logger;

    public DebugController(
        IECFRDataService ecfrDataService, 
        IFileStorageService fileStorageService,
        ILogger<DebugController> logger)
    {
        _ecfrDataService = ecfrDataService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var rawResponse = await _ecfrDataService.GetRawApiResponseAsync(10);
            ViewBag.RawResponse = rawResponse;
            
            var parsedData = await _ecfrDataService.FetchECFRDataAsync(10);
            ViewBag.ParsedData = parsedData;
            
            var currentData = await _fileStorageService.LoadDataAsync();
            ViewBag.CurrentData = currentData;
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in debug view");
            ViewBag.Error = ex.Message;
            return View();
        }
    }
}