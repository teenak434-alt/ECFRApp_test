# Testing Guide for ECFR Application

## Testing Instructions

After restarting the application, follow these steps to test the complete data flow:

### Step 1: Test API Connectivity
1. Navigate to `/Debug/Index`
2. The page should automatically call the API and display:
   - Raw JSON response from ECFR.gov
   - Parsed data showing document details
   - Current stored data (if any exists)

**Expected Result**: You should see JSON data with document information. Check that the parsed data has non-null values for Title, Agency, and other fields.

### Step 2: Fetch and Store Data
1. Navigate to `/Dashboard/Index`
2. Click the "Fetch Latest Data" button
3. Wait for the success message
4. Page should reload automatically

**Expected Result**: 
- Success message showing number of documents fetched
- Dashboard displays agency statistics
- Bar chart shows top 10 agencies
- Summary shows total documents and agencies

### Step 3: Verify Data Storage
1. Check the `Data` folder in your project directory
2. Verify `ecfr_current.json` exists and contains data
3. Verify `ecfr_historical.json` exists

**Expected Result**: Both files should exist with properly formatted JSON data.

### Step 4: Test Historical View
1. Navigate to `/Dashboard/Historical`
2. Verify the timeline chart displays
3. Use the agency filter dropdown
4. View the data table

**Expected Result**: 
- Chart showing data points over time
- Filter works to show specific agency data
- Table displays all historical records

### Step 5: Test Checksum View
1. Navigate to `/Dashboard/Checksums`
2. View the checksum for each agency
3. Click "Copy" button to copy a checksum
4. Paste the checksum into the verification tool
5. Click "Verify Checksum"

**Expected Result**: 
- Each agency shows a SHA-256 hash
- Copy button works
- Verification correctly identifies the agency

### Step 6: Test API Endpoints Directly

Use a tool like curl, Postman, or your browser:

```bash
# Get raw API response (first 10 results)
GET http://localhost:5000/api/ECFRData/raw?pageSize=10

# Get current statistics
GET http://localhost:5000/api/ECFRData/statistics

# Get historical data
GET http://localhost:5000/api/ECFRData/historical

# Get current snapshot
GET http://localhost:5000/api/ECFRData/snapshot

# Fetch new data (POST request)
POST http://localhost:5000/api/ECFRData/fetch?pageSize=100
```

## Common Issues and Solutions

### Issue: All data shows "Unknown" agency
**Solution**: 
1. Go to Debug page
2. Check raw API response to see actual field names
3. Logs will show "First item properties: ..." with available fields
4. If field names differ, update `ECFRDataService.cs` accordingly

### Issue: Data not saving
**Solution**:
1. Check if `Data` folder has write permissions
2. Look at application logs for errors
3. Verify `FileStorageService` is properly registered in DI

### Issue: Charts not displaying
**Solution**:
1. Check browser console for JavaScript errors
2. Verify Chart.js is loading from CDN
3. Ensure data is not empty

### Issue: Hot reload errors
**Solution**:
Stop and restart the application. Changes to interfaces require a full restart.

## Data Validation Checklist

After fetching data, verify:

- [ ] Dashboard shows multiple agencies (not just "Unknown")
- [ ] Document counts are greater than 0
- [ ] Last updated timestamp is recent
- [ ] Bar chart displays correctly
- [ ] Historical data accumulates on subsequent fetches
- [ ] Checksums are 64-character hexadecimal strings
- [ ] Debug page shows non-null field values

## Performance Notes

- Default fetch size: 100 documents
- Historical data: Max 100 entries per agency
- API calls may take 2-10 seconds depending on network
- File writes are async and should not block UI

## Logs to Monitor

Watch the application logs for:
- "Fetching ECFR data from: ..." - API call started
- "API Response (first 500 chars): ..." - Response preview
- "First item properties: ..." - Available field names
- "Successfully fetched {Count} documents" - Parse success
- "Data saved successfully to {File}" - Storage success

Any errors will appear with "Error fetching ECFR data" or similar messages.
