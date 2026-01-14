using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECFRApp_test.Models;

namespace ECFRApp_test.Services;

public interface IFileStorageService
{
    Task SaveDataAsync(ECFRDataSnapshot snapshot);
    Task<ECFRDataSnapshot?> LoadDataAsync();
    Task<List<AgencyStatistics>> GetAgencyStatisticsAsync();
    Task<List<HistoricalChange>> GetHistoricalChangesAsync();
    string CalculateChecksum(string data);
    Task CleanupHistoricalDataAsync();
}

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _dataDirectory;
    private readonly string _currentDataFile;
    private readonly string _historicalDataFile;

    public FileStorageService(ILogger<FileStorageService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        _currentDataFile = Path.Combine(_dataDirectory, "ecfr_current.json");
        _historicalDataFile = Path.Combine(_dataDirectory, "ecfr_historical.json");

        // Ensure directory exists
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    public async Task SaveDataAsync(ECFRDataSnapshot snapshot)
    {
        try
        {
            // Calculate statistics and checksums
            snapshot.Statistics = CalculateStatistics(snapshot.Documents);
            snapshot.FetchedAt = DateTime.UtcNow;

            // Save current snapshot
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(_currentDataFile, json);

            // Append to historical data
            await AppendHistoricalDataAsync(snapshot);

            _logger.LogInformation("Data saved successfully to {File}", _currentDataFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving data");
            throw;
        }
    }

    public async Task<ECFRDataSnapshot?> LoadDataAsync()
    {
        try
        {
            if (!File.Exists(_currentDataFile))
            {
                _logger.LogWarning("Data file not found: {File}", _currentDataFile);
                return null;
            }

            var json = await File.ReadAllTextAsync(_currentDataFile);
            return JsonSerializer.Deserialize<ECFRDataSnapshot>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data");
            return null;
        }
    }

    public async Task<List<AgencyStatistics>> GetAgencyStatisticsAsync()
    {
        var snapshot = await LoadDataAsync();
        return snapshot?.Statistics ?? new List<AgencyStatistics>();
    }

    public async Task<List<HistoricalChange>> GetHistoricalChangesAsync()
    {
        try
        {
            if (!File.Exists(_historicalDataFile))
            {
                return new List<HistoricalChange>();
            }

            var json = await File.ReadAllTextAsync(_historicalDataFile);
            return JsonSerializer.Deserialize<List<HistoricalChange>>(json) ?? new List<HistoricalChange>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading historical data");
            return new List<HistoricalChange>();
        }
    }

    public string CalculateChecksum(string data)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private List<AgencyStatistics> CalculateStatistics(List<ECFRDocument> documents)
    {
        var statistics = documents
            .GroupBy(d => d.AgencyName ?? "Unknown")
            .Select(g =>
            {
                var agencyData = JsonSerializer.Serialize(g.ToList());
                return new AgencyStatistics
                {
                    AgencyName = g.Key,
                    DocumentCount = g.Count(),
                    Checksum = CalculateChecksum(agencyData),
                    LastUpdated = DateTime.UtcNow
                };
            })
            .OrderByDescending(s => s.DocumentCount)
            .ToList();

        return statistics;
    }

    private async Task AppendHistoricalDataAsync(ECFRDataSnapshot snapshot)
    {
        try
        {
            var historicalData = await GetHistoricalChangesAsync();

            // Add new entries
            var newEntries = snapshot.Statistics.Select(s => new HistoricalChange
            {
                AgencyName = s.AgencyName,
                Date = snapshot.FetchedAt,
                DocumentCount = s.DocumentCount
            }).ToList();

            historicalData.AddRange(newEntries);

            // Keep only last 100 entries per agency
            historicalData = historicalData
                .GroupBy(h => h.AgencyName)
                .SelectMany(g => g.OrderByDescending(h => h.Date).Take(100))
                .OrderBy(h => h.Date)
                .ToList();

            var json = JsonSerializer.Serialize(historicalData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(_historicalDataFile, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error appending historical data");
        }
    }

    public async Task CleanupHistoricalDataAsync()
    {
        try
        {
            var historicalData = await GetHistoricalChangesAsync();
            
            // Remove entries with invalid agency names
            var invalidNames = new[] { "Unknown", "Unknown Agency", "1", "10", "2", "3", "4", "5", "6", "7", "8", "9" };
            
            var cleanedData = historicalData
                .Where(h => !invalidNames.Contains(h.AgencyName))
                .OrderBy(h => h.Date)
                .ToList();

            if (cleanedData.Count != historicalData.Count)
            {
                _logger.LogInformation("Cleaned up {Count} invalid historical entries", historicalData.Count - cleanedData.Count);
                
                var json = JsonSerializer.Serialize(cleanedData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await File.WriteAllTextAsync(_historicalDataFile, json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up historical data");
        }
    }
}