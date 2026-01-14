# API Documentation - Reading ECFR Data from JSON Files

## Overview

This application provides REST API endpoints to read stored ECFR data from JSON files. All data is fetched from ECFR.gov and stored locally in the `Data` folder.

## Data Files

### Location
- **Current Data**: `Data/ecfr_current.json`
- **Historical Data**: `Data/ecfr_historical.json`

### File Structure

#### ecfr_current.json
```json
{
  "FetchedAt": "2026-01-04T03:21:53.7603417Z",
  "Documents": [
    {
      "DocumentNumber": "string",
      "Title": "string",
      "AgencyName": "string",
      "Type": "string",
      "PublicationDate": "datetime",
      "Citation": "string",
      "Url": "string"
    }
  ],
  "Statistics": [
    {
      "AgencyName": "string",
      "DocumentCount": 100,
      "Checksum": "sha256-hash",
      "LastUpdated": "datetime"
    }
  ]
}
```

#### ecfr_historical.json
```json
[
  {
    "AgencyName": "string",
    "Date": "datetime",
    "DocumentCount": 100
  }
]
```

## API Endpoints

### 1. Get Current Snapshot

**Endpoint**: `GET /api/ECFRData/snapshot`

**Description**: Returns the complete current data snapshot from `ecfr_current.json`.

**Response**:
```json
{
  "fetchedAt": "2026-01-04T03:21:53.7603417Z",
  "documents": [
    {
      "documentNumber": null,
      "title": null,
      "agencyName": "Unknown",
      "type": "Section",
      "publicationDate": null,
      "citation": null,
      "url": null
    }
  ],
  "statistics": [
    {
      "agencyName": "Unknown",
      "documentCount": 100,
      "checksum": "8d60dd005471ea86ed6b1c3495c8ae2b276a581e6e8ff848d07b2d71a1e74ade",
      "lastUpdated": "2026-01-04T03:21:53.7602783Z"
    }
  ]
}
```

**Status Codes**:
- `200 OK` - Success
- `404 Not Found` - No data file exists
- `500 Internal Server Error` - Server error

**Example Usage**:
```javascript
fetch('/api/ECFRData/snapshot')
  .then(response => response.json())
  .then(data => console.log(data));
```

### 2. Get Agency Statistics

**Endpoint**: `GET /api/ECFRData/statistics`

**Description**: Returns agency statistics from the Statistics section of `ecfr_current.json`.

**Response**:
```json
[
  {
    "agencyName": "Unknown",
    "documentCount": 100,
    "checksum": "8d60dd005471ea86ed6b1c3495c8ae2b276a581e6e8ff848d07b2d71a1e74ade",
    "lastUpdated": "2026-01-04T03:21:53.7602783Z"
  }
]
```

**Status Codes**:
- `200 OK` - Success (returns empty array if no data)
- `500 Internal Server Error` - Server error

**Example Usage**:
```javascript
fetch('/api/ECFRData/statistics')
  .then(response => response.json())
  .then(stats => {
    stats.forEach(stat => {
      console.log(`${stat.agencyName}: ${stat.documentCount} documents`);
    });
  });
```

### 3. Get Historical Changes

**Endpoint**: `GET /api/ECFRData/historical`

**Description**: Returns historical change data from `ecfr_historical.json`.

**Response**:
```json
[
  {
    "agencyName": "Unknown",
    "date": "2026-01-04T03:21:53.7602783Z",
    "documentCount": 100
  }
]
```

**Status Codes**:
- `200 OK` - Success (returns empty array if no data)
- `500 Internal Server Error` - Server error

**Example Usage**:
```javascript
fetch('/api/ECFRData/historical')
  .then(response => response.json())
  .then(history => {
    // Group by agency
    const byAgency = history.reduce((acc, item) => {
      if (!acc[item.agencyName]) acc[item.agencyName] = [];
      acc[item.agencyName].push(item);
      return acc;
    }, {});
    console.log(byAgency);
  });
```

## UI Pages That Display This Data

### 1. Dashboard (`/Dashboard/Index`)
- Reads: `GET /api/ECFRData/statistics`
- Displays: Agency document counts, bar chart, summary
- Features: Fetch new data button

### 2. Data Viewer (`/DataViewer/Index`)
- Reads: Direct file access via `IFileStorageService`
- Displays: Complete snapshot, all documents, statistics
- Features: Search, filter, detailed view

### 3. Historical View (`/Dashboard/Historical`)
- Reads: `GET /api/ECFRData/historical`
- Displays: Timeline chart, historical data table
- Features: Agency filter, trend visualization

### 4. Checksums View (`/Dashboard/Checksums`)
- Reads: `GET /api/ECFRData/statistics`
- Displays: SHA-256 checksums per agency
- Features: Copy checksum, verification tool

### 5. Debug View (`/Debug/Index`)
- Reads: Multiple sources (raw API, parsed data, stored data)
- Displays: Complete debugging information
- Features: Raw JSON inspection

## Integration Examples

### React/JavaScript Frontend
```javascript
class ECFRDataService {
  async getSnapshot() {
    const response = await fetch('/api/ECFRData/snapshot');
    if (!response.ok) throw new Error('Failed to fetch snapshot');
    return await response.json();
  }

  async getStatistics() {
    const response = await fetch('/api/ECFRData/statistics');
    return await response.json();
  }

  async getHistorical() {
    const response = await fetch('/api/ECFRData/historical');
    return await response.json();
  }
}

// Usage
const service = new ECFRDataService();
const stats = await service.getStatistics();
```

### C# HttpClient
```csharp
using System.Net.Http.Json;

public class ECFRClient
{
    private readonly HttpClient _httpClient;

    public ECFRClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ECFRDataSnapshot?> GetSnapshotAsync()
    {
        return await _httpClient.GetFromJsonAsync<ECFRDataSnapshot>(
            "/api/ECFRData/snapshot");
    }

    public async Task<List<AgencyStatistics>> GetStatisticsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AgencyStatistics>>(
            "/api/ECFRData/statistics") ?? new();
    }

    public async Task<List<HistoricalChange>> GetHistoricalAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<HistoricalChange>>(
            "/api/ECFRData/historical") ?? new();
    }
}
```

### PowerShell
```powershell
# Get snapshot
$snapshot = Invoke-RestMethod -Uri "http://localhost:5000/api/ECFRData/snapshot" -Method Get

# Get statistics
$stats = Invoke-RestMethod -Uri "http://localhost:5000/api/ECFRData/statistics" -Method Get
$stats | Format-Table AgencyName, DocumentCount, Checksum

# Get historical data
$history = Invoke-RestMethod -Uri "http://localhost:5000/api/ECFRData/historical" -Method Get
$history | Group-Object AgencyName | Select-Object Name, Count
```

### curl
```bash
# Get snapshot
curl http://localhost:5000/api/ECFRData/snapshot

# Get statistics
curl http://localhost:5000/api/ECFRData/statistics | jq '.[] | {agencyName, documentCount}'

# Get historical data
curl http://localhost:5000/api/ECFRData/historical | jq 'group_by(.agencyName)'
```

## Service Layer Access

If you're building additional controllers or services within the application, you can access the data through the `IFileStorageService`:

```csharp
public class MyController : Controller
{
    private readonly IFileStorageService _fileStorageService;

    public MyController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<IActionResult> MyAction()
    {
        // Get complete snapshot
        var snapshot = await _fileStorageService.LoadDataAsync();
        
        // Get statistics only
        var statistics = await _fileStorageService.GetAgencyStatisticsAsync();
        
        // Get historical data
        var historical = await _fileStorageService.GetHistoricalChangesAsync();
        
        return View(snapshot);
    }
}
```

## Error Handling

All endpoints follow standard HTTP status codes:

- **200 OK**: Request succeeded, data returned
- **404 Not Found**: Data file doesn't exist (for snapshot endpoint)
- **500 Internal Server Error**: Server error occurred

Error responses include a JSON body:
```json
{
  "success": false,
  "message": "Error message here"
}
```

## Testing

Visit `/Home/ApiTest` for an interactive API testing page where you can:
- Test all endpoints
- View response times
- Inspect JSON responses
- See metadata about responses

## Notes

- Data is read-only through these endpoints
- To update data, use `POST /api/ECFRData/fetch`
- All timestamps are in UTC
- Checksums are SHA-256 hashes in lowercase hexadecimal
- Historical data is limited to 100 entries per agency
