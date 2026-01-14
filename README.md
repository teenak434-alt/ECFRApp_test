# ECFR Data Integration Application

This ASP.NET Core MVC application fetches data from the ECFR.gov (Electronic Code of Federal Regulations) public API, stores it locally, and provides a comprehensive UI to visualize and analyze the data.

## Features

### 1. Data Fetching
- Fetches data from the ECFR.gov API: `https://www.ecfr.gov/api/search/v1/results`
- Stores fetched data in JSON files for offline access
- Configurable page size for API requests
- Enhanced field parsing with multiple field name attempts for better compatibility

### 2. Dashboard Views

#### Home Page (`/Home/Index`)
- **Quick Access**: Links to all features and views
- **API Documentation**: Overview of available endpoints
- **Getting Started**: Clear navigation to key features

#### Dashboard (`/Dashboard/Index`)
- **Count per Agency**: Displays document count for each agency
- **Summary Statistics**: Total agencies, total documents, last update time
- **Interactive Bar Chart**: Visual representation of top 10 agencies
- **Fetch Latest Data**: Button to fetch fresh data from ECFR.gov

#### Data Viewer (`/DataViewer/Index`)
- **Complete Data View**: Display all data from ecfr_current.json
- **Document Browser**: Search and filter through all documents
- **Type Breakdown**: Visual breakdown of document types
- **Agency Statistics**: Detailed statistics with checksums
- **Raw JSON Access**: Direct link to view raw JSON data

#### Historical Changes (`/Dashboard/Historical`)
- **Timeline Visualization**: Line chart showing document count changes over time
- **Per-Agency Tracking**: Filter and view historical data by agency
- **Data Table**: Detailed listing of all historical data points

#### Checksums (`/Dashboard/Checksums`)
- **SHA-256 Checksums**: Calculated for each agency's data
- **Data Integrity Verification**: Verify checksums to ensure data hasn't changed
- **Copy to Clipboard**: Easy copying of checksum values
- **Checksum Validator**: Input a checksum to verify which agency it belongs to

#### Debug View (`/Debug/Index`)
- **Raw API Response**: View the actual JSON returned from ECFR API
- **Parsed Data Inspection**: See how data is being parsed from the API
- **Current Stored Data**: View what's currently in the data files
- **Troubleshooting Tool**: Helps diagnose any data fetching or parsing issues

#### API Test Page (`/Home/ApiTest`)
- **Interactive Testing**: Test all API endpoints
- **Response Viewer**: See formatted JSON responses
- **Performance Metrics**: View response times
- **Quick Links**: Direct access to all APIs

### 3. API Endpoints

All API endpoints are available under `/api/ECFRData`:

- `POST /api/ECFRData/fetch?pageSize=100` - Fetch and save new data from ECFR.gov
- `GET /api/ECFRData/snapshot` - Get the complete current data snapshot from ecfr_current.json
- `GET /api/ECFRData/statistics` - Get current agency statistics from ecfr_current.json
- `GET /api/ECFRData/historical` - Get historical changes data from ecfr_historical.json
- `GET /api/ECFRData/raw?pageSize=10` - Get raw API response for debugging

**See [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for detailed API documentation.**

## Project Structure

```
ECFRApp_test/
??? Controllers/
?   ??? DashboardController.cs      # UI controller for dashboard views
?   ??? ECFRDataController.cs       # API controller for data operations
?   ??? DebugController.cs          # Debug view controller
?   ??? HomeController.cs           # Default home controller
??? Models/
?   ??? ECFRModels.cs              # Data models for ECFR API
?   ??? ErrorViewModel.cs          # Error view model
??? Services/
?   ??? ECFRDataService.cs         # Service to fetch data from ECFR API
?   ??? FileStorageService.cs      # Service to persist and retrieve data
??? Views/
?   ??? Dashboard/
?   ?   ??? Index.cshtml           # Main dashboard view
?   ?   ??? Historical.cshtml      # Historical changes view
?   ?   ??? Checksums.cshtml       # Checksums view
?   ??? Debug/
?   ?   ??? Index.cshtml           # Debug information view
?   ??? Home/
?   ?   ??? Index.cshtml           # Home page with feature links
?   ?   ??? ApiTest.cshtml         # API test page
?   ??? Shared/
?       ??? _Layout.cshtml          # Shared layout with navigation
??? Data/                           # Created at runtime
    ??? ecfr_current.json          # Current data snapshot
    ??? ecfr_historical.json       # Historical data records
```

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 or later / VS Code

### Running the Application

1. **Build the solution**:
   ```bash
   dotnet build
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```
   
   **Note**: If you modified code while debugging, you'll need to restart the application to see the changes.

3. **Access the application**:
   - Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`
   - Click on "Dashboard" in the navigation menu

4. **Fetch Initial Data**:
   - On the Dashboard page, click the "Fetch Latest Data" button
   - Wait for the data to be fetched from ECFR.gov
   - The page will automatically refresh to show the results

## Usage Guide

### Fetching Data
1. Navigate to the Dashboard
2. Click "Fetch Latest Data"
3. The system will:
   - Call the ECFR.gov API
   - Parse and process the results
   - Calculate statistics and checksums
   - Store data in JSON files
   - Update historical tracking

### Viewing Statistics
- The Dashboard shows all agencies ranked by document count
- View interactive charts showing the top 10 agencies
- See summary statistics at the top

### Tracking Historical Changes
1. Navigate to "Historical" in the menu
2. View the timeline chart showing changes over time
3. Filter by specific agency using the dropdown
4. Historical data accumulates with each fetch

### Verifying Checksums
1. Navigate to "Checksums" in the menu
2. View SHA-256 checksums for each agency
3. Copy checksums using the "Copy" button
4. Verify checksums by pasting them into the verification tool

### Debugging Data Issues
1. Navigate to "Debug" in the menu
2. View the raw API response to see what ECFR.gov returns
3. Check how the data is being parsed
4. Verify what's stored in the data files
5. Use this information to troubleshoot any issues

### Using the Data Viewer
1. Navigate to "Data Viewer" in the menu
2. View all data from ecfr_current.json in a structured format
3. Use the search and filter options to find specific documents
4. Explore the visual breakdown of document types and agency statistics
5. Access raw JSON data directly for advanced analysis

### Testing APIs
1. Navigate to the API Test page
2. Select an API endpoint to test
3. View the request and response format
4. Analyze performance metrics for each API call

## Data Storage

Data is stored in the `Data` folder (created automatically):
- **ecfr_current.json**: Current snapshot with all documents and statistics
- **ecfr_historical.json**: Historical records (max 100 entries per agency)

## API Response Handling

The application handles multiple possible field names from the ECFR API:
- **Document Number**: `document_number`, `documentNumber`, `doc_number`, `object_id`
- **Title**: `title`, `heading`, `section_id`
- **Agency**: `agency_names`, `agencies`, `agency`, or extracted from `hierarchy`
- **Type**: `type`, `document_type`
- **Citation**: `citation`, `cfr_reference`
- **URL**: `html_url`, `url`
- **Date**: `publication_date`, `date`

This flexible parsing ensures compatibility with the ECFR API structure.

## Technologies Used

- **ASP.NET Core 10.0**: Web framework
- **C# 13**: Programming language
- **Bootstrap 5**: UI framework
- **Chart.js**: Interactive charts
- **System.Text.Json**: JSON processing
- **System.Security.Cryptography**: SHA-256 checksum calculation

## Troubleshooting

### Data showing as "Unknown" agency
- Visit the Debug page (`/Debug/Index`) to see the raw API response
- Check the logs for field mapping information
- The API may return data in a different format than expected

### No data appearing after fetch
- Check the Debug page to verify data is being received
- Look at the browser console for JavaScript errors
- Verify the Data folder exists and has write permissions

### Build errors after code changes while debugging
- Stop the application and rebuild
- Some changes (like adding interface methods) require a full restart
- Hot reload has limitations with structural changes

## License

This project is for educational and demonstration purposes.
