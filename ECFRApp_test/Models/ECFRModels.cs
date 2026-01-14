namespace ECFRApp_test.Models;

public class ECFRSearchResult
{
    public int TotalResults { get; set; }
    public List<ECFRDocument> Results { get; set; } = new();
}

public class ECFRDocument
{
    public string? DocumentNumber { get; set; }
    public string? Title { get; set; }
    public string? AgencyName { get; set; }
    public string? Type { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string? Citation { get; set; }
    public string? Url { get; set; }
}

public class AgencyStatistics
{
    public string AgencyName { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class HistoricalChange
{
    public string AgencyName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int DocumentCount { get; set; }
}

public class ECFRDataSnapshot
{
    public DateTime FetchedAt { get; set; }
    public List<ECFRDocument> Documents { get; set; } = new();
    public List<AgencyStatistics> Statistics { get; set; } = new();
}