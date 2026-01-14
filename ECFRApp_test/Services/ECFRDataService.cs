using System.Text.Json;
using ECFRApp_test.Models;

namespace ECFRApp_test.Services;

public interface IECFRDataService
{
    Task<ECFRSearchResult> FetchECFRDataAsync(int pageSize = 100);
    Task<string> GetRawApiResponseAsync(int pageSize = 100);
}

public class ECFRDataService : IECFRDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ECFRDataService> _logger;
    private const string ECFR_API_URL = "https://www.ecfr.gov/api/search/v1/results";

    public ECFRDataService(HttpClient httpClient, ILogger<ECFRDataService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetRawApiResponseAsync(int pageSize = 100)
    {
        try
        {
            var url = $"{ECFR_API_URL}?per_page={pageSize}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching raw ECFR data");
            throw;
        }
    }

    public async Task<ECFRSearchResult> FetchECFRDataAsync(int pageSize = 100)
    {
        try
        {
            var url = $"{ECFR_API_URL}?per_page={pageSize}";
            _logger.LogInformation("Fetching ECFR data from: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            
            // Log first 500 characters for debugging
            _logger.LogInformation("API Response (first 500 chars): {Response}", 
                content.Length > 500 ? content.Substring(0, 500) : content);
            
            // Parse the JSON response
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            var result = new ECFRSearchResult();

            // Try different possible property names for total count
            if (root.TryGetProperty("total_count", out var totalCount))
            {
                result.TotalResults = totalCount.GetInt32();
            }
            else if (root.TryGetProperty("count", out totalCount))
            {
                result.TotalResults = totalCount.GetInt32();
            }

            if (root.TryGetProperty("results", out var results))
            {
                _logger.LogInformation("Found 'results' array with {Count} items", results.GetArrayLength());
                
                int itemIndex = 0;
                foreach (var item in results.EnumerateArray())
                {
                    // Log first item properties for debugging
                    if (itemIndex == 0)
                    {
                        _logger.LogInformation("First item properties: {Props}", 
                            string.Join(", ", item.EnumerateObject().Select(p => p.Name)));
                    }

                    var doc = new ECFRDocument
                    {
                        // Try multiple possible field names
                        DocumentNumber = GetPropertyString(item, "document_number") 
                            ?? GetPropertyString(item, "documentNumber")
                            ?? GetPropertyString(item, "doc_number")
                            ?? GetPropertyString(item, "object_id"),
                        
                        // Try to get section title from headings.section first
                        Title = ExtractTitle(item),
                        
                        AgencyName = ExtractAgencyName(item),
                        
                        Type = GetPropertyString(item, "type") 
                            ?? GetPropertyString(item, "document_type")
                            ?? "Unknown",
                        
                        Citation = ExtractCitation(item),
                        
                        Url = GetPropertyString(item, "html_url") 
                            ?? GetPropertyString(item, "url")
                    };

                    // Try to get publication date
                    if (item.TryGetProperty("starts_on", out var startsOn))
                    {
                        if (DateTime.TryParse(startsOn.GetString(), out var date))
                        {
                            doc.PublicationDate = date;
                        }
                    }
                    else if (item.TryGetProperty("publication_date", out var pubDate))
                    {
                        if (DateTime.TryParse(pubDate.GetString(), out var date))
                        {
                            doc.PublicationDate = date;
                        }
                    }
                    else if (item.TryGetProperty("date", out pubDate))
                    {
                        if (DateTime.TryParse(pubDate.GetString(), out var date))
                        {
                            doc.PublicationDate = date;
                        }
                    }

                    result.Results.Add(doc);
                    itemIndex++;
                }
            }

            _logger.LogInformation("Successfully fetched {Count} documents from ECFR", result.Results.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ECFR data");
            throw;
        }
    }

    private string ExtractAgencyName(JsonElement item)
    {
        // First, try to get from 'headings' object - this has the actual descriptive names
        if (item.TryGetProperty("headings", out var headings))
        {
            // Try chapter first (usually the agency/organization name)
            if (headings.TryGetProperty("chapter", out var chapter))
            {
                var chapterStr = chapter.GetString();
                if (!string.IsNullOrEmpty(chapterStr))
                {
                    return chapterStr;
                }
            }

            // Try title as fallback
            if (headings.TryGetProperty("title", out var titleProp))
            {
                var titleStr = titleProp.GetString();
                if (!string.IsNullOrEmpty(titleStr))
                {
                    return titleStr;
                }
            }
        }

        // Try multiple possible field names for direct agency properties
        var agencyStr = GetPropertyString(item, "agency_names") 
            ?? GetPropertyString(item, "agencies")
            ?? GetPropertyString(item, "agency");

        if (!string.IsNullOrEmpty(agencyStr) && !int.TryParse(agencyStr, out _))
        {
            return agencyStr.Split(',').FirstOrDefault()?.Trim() ?? "Unknown Agency";
        }

        // Try to extract from hierarchy - look for agency information
        if (item.TryGetProperty("hierarchy", out var hierarchy))
        {
            // Try to get title number and map it
            if (hierarchy.TryGetProperty("title", out var titleNum))
            {
                var titleNumber = titleNum.GetString();
                var agencyName = MapTitleNumberToAgency(titleNumber);
                if (!string.IsNullOrEmpty(agencyName))
                {
                    return agencyName;
                }
            }
        }

        // Try to get structured agency info
        if (item.TryGetProperty("agency", out var agencyObj))
        {
            if (agencyObj.ValueKind == JsonValueKind.Object)
            {
                if (agencyObj.TryGetProperty("name", out var name))
                {
                    var nameStr = name.GetString();
                    if (!string.IsNullOrEmpty(nameStr))
                    {
                        return nameStr;
                    }
                }
            }
            else if (agencyObj.ValueKind == JsonValueKind.String)
            {
                var agencyString = agencyObj.GetString();
                if (!string.IsNullOrEmpty(agencyString) && !int.TryParse(agencyString, out _))
                {
                    return agencyString;
                }
            }
        }

        // Try parent agency
        if (item.TryGetProperty("parent_agency", out var parentAgency))
        {
            var parentStr = parentAgency.GetString();
            if (!string.IsNullOrEmpty(parentStr) && !int.TryParse(parentStr, out _))
            {
                return parentStr;
            }
        }

        return "Unknown Agency";
    }

    private static string? MapTitleNumberToAgency(string? titleNumber)
    {
        if (string.IsNullOrEmpty(titleNumber))
            return null;

        // Map CFR Title numbers to their corresponding agencies
        // Based on https://www.ecfr.gov/
        return titleNumber switch
        {
            "1" => "General Provisions",
            "2" => "Grants and Agreements",
            "3" => "The President",
            "4" => "Accounts",
            "5" => "Administrative Personnel",
            "6" => "Domestic Security",
            "7" => "Agriculture",
            "8" => "Aliens and Nationality",
            "9" => "Animals and Animal Products",
            "10" => "Energy",
            "11" => "Federal Elections",
            "12" => "Banks and Banking",
            "13" => "Business Credit and Assistance",
            "14" => "Aeronautics and Space",
            "15" => "Commerce and Foreign Trade",
            "16" => "Commercial Practices",
            "17" => "Commodity and Securities Exchanges",
            "18" => "Conservation of Power and Water Resources",
            "19" => "Customs Duties",
            "20" => "Employees' Benefits",
            "21" => "Food and Drugs",
            "22" => "Foreign Relations",
            "23" => "Highways",
            "24" => "Housing and Urban Development",
            "25" => "Indians",
            "26" => "Internal Revenue",
            "27" => "Alcohol, Tobacco and Firearms",
            "28" => "Judicial Administration",
            "29" => "Labor",
            "30" => "Mineral Resources",
            "31" => "Money and Finance: Treasury",
            "32" => "National Defense",
            "33" => "Navigation and Navigable Waters",
            "34" => "Education",
            "36" => "Parks, Forests, and Public Property",
            "37" => "Patents, Trademarks, and Copyrights",
            "38" => "Pensions, Bonuses, and Veterans' Relief",
            "39" => "Postal Service",
            "40" => "Protection of Environment",
            "41" => "Public Contracts and Property Management",
            "42" => "Public Health",
            "43" => "Public Lands: Interior",
            "44" => "Emergency Management and Assistance",
            "45" => "Public Welfare",
            "46" => "Shipping",
            "47" => "Telecommunication",
            "48" => "Federal Acquisition Regulations System",
            "49" => "Transportation",
            "50" => "Wildlife and Fisheries",
            _ => $"Title {titleNumber}"
        };
    }
    
    private static string? GetPropertyString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
            else if (property.ValueKind == JsonValueKind.Number)
            {
                return property.ToString();
            }
            else if (property.ValueKind == JsonValueKind.Object || property.ValueKind == JsonValueKind.Array)
            {
                // Don't try to stringify objects/arrays
                return null;
            }
            return property.ToString();
        }
        return null;
    }

    private string? ExtractTitle(JsonElement item)
    {
        // Try headings.section first (has the actual section title)
        if (item.TryGetProperty("headings", out var headings))
        {
            if (headings.TryGetProperty("section", out var section))
            {
                var sectionStr = section.GetString();
                if (!string.IsNullOrEmpty(sectionStr))
                {
                    return sectionStr;
                }
            }

            // Try part title
            if (headings.TryGetProperty("part", out var part))
            {
                var partStr = part.GetString();
                if (!string.IsNullOrEmpty(partStr))
                {
                    return partStr;
                }
            }
        }

        // Fallback to standard fields
        return GetPropertyString(item, "title") 
            ?? GetPropertyString(item, "heading")
            ?? GetPropertyString(item, "section_id");
    }

    private string? ExtractCitation(JsonElement item)
    {
        // Try to build citation from hierarchy_headings
        if (item.TryGetProperty("hierarchy_headings", out var hierarchyHeadings))
        {
            var parts = new List<string>();

            if (hierarchyHeadings.TryGetProperty("title", out var title))
            {
                var titleStr = title.GetString();
                if (!string.IsNullOrEmpty(titleStr))
                    parts.Add(titleStr);
            }

            if (hierarchyHeadings.TryGetProperty("part", out var part))
            {
                var partStr = part.GetString();
                if (!string.IsNullOrEmpty(partStr))
                    parts.Add(partStr);
            }

            if (hierarchyHeadings.TryGetProperty("section", out var section))
            {
                var sectionStr = section.GetString();
                if (!string.IsNullOrEmpty(sectionStr))
                    parts.Add(sectionStr);
            }

            if (parts.Any())
            {
                return string.Join(" ", parts);
            }
        }

        // Fallback to standard fields
        return GetPropertyString(item, "citation") 
            ?? GetPropertyString(item, "cfr_reference");
    }
}