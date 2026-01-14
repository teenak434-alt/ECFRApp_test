# Quick Reference Guide - Reading ECFR Data

## Files to Read From

```
Data/ecfr_current.json      ? Current snapshot with all documents and statistics
Data/ecfr_historical.json   ? Historical changes over time
```

## API Endpoints (Read Data)

| Endpoint | Returns | UI Page |
|----------|---------|---------|
| `GET /api/ECFRData/snapshot` | Complete snapshot | [Data Viewer](/DataViewer/Index) |
| `GET /api/ECFRData/statistics` | Agency statistics | [Dashboard](/Dashboard/Index) |
| `GET /api/ECFRData/historical` | Historical changes | [Historical](/Dashboard/Historical) |

## UI Pages (Display Data)

| Page | URL | What It Shows |
|------|-----|---------------|
| **Home** | `/Home/Index` | Overview and navigation |
| **Dashboard** | `/Dashboard/Index` | Statistics, counts, charts |
| **Data Viewer** | `/DataViewer/Index` | Complete data view, all documents |
| **Historical** | `/Dashboard/Historical` | Timeline charts, historical trends |
| **Checksums** | `/Dashboard/Checksums` | Data integrity verification |
| **Debug** | `/Debug/Index` | Troubleshooting and raw data |
| **API Test** | `/Home/ApiTest` | Interactive API testing |

## Quick Start

### View Stored Data in UI
1. Navigate to **Data Viewer** (`/DataViewer/Index`)
2. See all documents, statistics, and type breakdown
3. Use search to filter documents

### Access Data via API
```javascript
// Get all data
const response = await fetch('/api/ECFRData/snapshot');
const data = await response.json();
console.log(data.documents);
console.log(data.statistics);

// Get statistics only
const stats = await fetch('/api/ECFRData/statistics');
const agencyStats = await stats.json();

// Get historical data
const history = await fetch('/api/ECFRData/historical');
const changes = await history.json();
```

### Read Files Directly (Server-Side)
```csharp
public class MyController : Controller
{
    private readonly IFileStorageService _fileStorageService;

    public MyController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<IActionResult> Index()
    {
        var snapshot = await _fileStorageService.LoadDataAsync();
        return View(snapshot);
    }
}
```

## Data Structure

### Snapshot (ecfr_current.json)
```json
{
  "fetchedAt": "2026-01-04T03:21:53Z",
  "documents": [ /* array of documents */ ],
  "statistics": [ /* array of agency stats */ ]
}
```

### Document Object
```json
{
  "documentNumber": "string or null",
  "title": "string or null",
  "agencyName": "string",
  "type": "Section/Appendix",
  "publicationDate": "datetime or null",
  "citation": "string or null",
  "url": "string or null"
}
```

### Agency Statistics Object
```json
{
  "agencyName": "string",
  "documentCount": 100,
  "checksum": "sha256-hash",
  "lastUpdated": "datetime"
}
```

### Historical Change Object (ecfr_historical.json)
```json
{
  "agencyName": "string",
  "date": "datetime",
  "documentCount": 100
}
```

## Common Tasks

### Display All Documents
```
UI: /DataViewer/Index
API: GET /api/ECFRData/snapshot
Service: await _fileStorageService.LoadDataAsync()
```

### Show Agency Statistics
```
UI: /Dashboard/Index
API: GET /api/ECFRData/statistics
Service: await _fileStorageService.GetAgencyStatisticsAsync()
```

### View Historical Trends
```
UI: /Dashboard/Historical
API: GET /api/ECFRData/historical
Service: await _fileStorageService.GetHistoricalChangesAsync()
```

### Verify Data Integrity
```
UI: /Dashboard/Checksums
API: GET /api/ECFRData/statistics (checksums included)
Service: _fileStorageService.CalculateChecksum(data)
```

## Testing

### Test APIs Interactively
Navigate to `/Home/ApiTest` and click "Test" buttons

### Test via Browser
- Snapshot: `http://localhost:5000/api/ECFRData/snapshot`
- Statistics: `http://localhost:5000/api/ECFRData/statistics`
- Historical: `http://localhost:5000/api/ECFRData/historical`

### Test via Command Line
```bash
# PowerShell
Invoke-RestMethod http://localhost:5000/api/ECFRData/statistics

# curl
curl http://localhost:5000/api/ECFRData/snapshot | jq
```

## Tips

? **Current Data**: Use `/api/ECFRData/snapshot` for complete current state  
? **Statistics Only**: Use `/api/ECFRData/statistics` for lighter response  
? **UI Views**: All UI pages automatically read and display stored data  
? **No Auth Required**: All read endpoints are public  
? **JSON Format**: All responses are in JSON format  

?? **Data Quality**: If showing "Unknown" agencies, visit Debug page  
?? **File Not Found**: Returns 404 if no data fetched yet  
?? **Timestamps**: All times are in UTC  

## Next Steps

1. **Fetch Data**: Click "Fetch Latest Data" on Dashboard
2. **View in UI**: Navigate to Data Viewer
3. **Access via API**: Use endpoints in your application
4. **Monitor Changes**: Check Historical view after multiple fetches

For detailed API documentation, see [API_DOCUMENTATION.md](API_DOCUMENTATION.md)
