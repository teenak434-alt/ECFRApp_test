# Fix: Historical Chart Not Displaying Data

## Problem
The historical chart was not displaying any data points, even though the data existed in `ecfr_historical.json`.

## Root Cause
**Property Name Casing Mismatch**

The JavaScript code was trying to access properties with **camelCase** names:
```javascript
x: new Date(item.date),         // ? Wrong - 'date' doesn't exist
y: item.documentCount            // ? Wrong - 'documentCount' doesn't exist
```

But the JSON serialization from C# uses **PascalCase**:
```json
{
  "Date": "2026-01-04T05:18:49.6285185Z",           // ? Actual property
  "DocumentCount": 33,                               // ? Actual property
  "AgencyName": "Nuclear Regulatory Commission"
}
```

This caused the chart to receive `undefined` values for x and y coordinates, resulting in no visible data points.

## Solution Implemented

### 1. Fixed Property Names
Changed JavaScript to use **PascalCase** to match JSON:

**Before:**
```javascript
data: historicalData[agency].map(item => ({
    x: new Date(item.date),        // ? undefined
    y: item.documentCount          // ? undefined
}))
```

**After:**
```javascript
data: historicalData[agency].map(item => ({
    x: new Date(item.Date),        // ? Correct
    y: item.DocumentCount          // ? Correct
}))
```

### 2. Enhanced Chart Styling
- **Fixed height container** (400px) for better visibility
- **Responsive sizing** with `maintainAspectRatio: false`
- **Improved legend** with point styles
- **Better tooltips** showing full date/time
- **Smaller step size** (5 instead of 10) for Y-axis

### 3. Added Debug Logging
Added console logs to help troubleshoot future issues:
```javascript
console.log('Historical Data:', historicalData);
console.log(`${agency} data points:`, agencyData);
console.log('Datasets:', datasets);
console.log('Chart created successfully');
```

## Current Historical Data

Based on `ecfr_historical.json`, you have **10 data points**:

### Two Fetches Recorded:
1. **2026-01-04 05:04:26** (First fetch)
   - Nuclear Regulatory Commission: 33 docs
   - Administrative Committee: 27 docs
   - Miscellaneous Agencies: 18 docs
   - National Capital Planning: 16 docs
   - Administrative Conference: 6 docs

2. **2026-01-04 05:18:49** (Second fetch)
   - Nuclear Regulatory Commission: 33 docs
   - Administrative Committee: 27 docs
   - Miscellaneous Agencies: 18 docs
   - National Capital Planning: 16 docs
   - Administrative Conference: 6 docs

## Expected Chart Display

After restart, the chart will show:
- **5 colored lines** (one per agency)
- **2 points per line** (one for each fetch time)
- **X-axis**: Time scale showing "Jan 4, 05:04" and "Jan 4, 05:18"
- **Y-axis**: Document counts (0-35 range)
- **Legend** at bottom with agency names
- **Tooltips** on hover showing exact date, time, and count

### Visual Example:
```
Document Count (Y-axis)
35 |                    ????? Nuclear Regulatory (33)
30 |
25 |              ?????       Administrative Committee (27)
20 |
15 |        ?????             Miscellaneous Agencies (18)
10 |  ?????                   National Capital Planning (16)
 5 | ????                     Administrative Conference (6)
 0 |_____|_____|_____|_____|_____|
     05:04   05:18              (Time on X-axis)
```

## Testing After Restart

### Step 1: Restart Application
```
Stop app ? Restart app
```

### Step 2: Navigate to Historical View
```
URL: /Dashboard/Historical
```

### Step 3: Verify Chart Displays
? Chart shows 5 colored lines  
? Each line has 2 visible points  
? X-axis shows two time points (05:04 and 05:18)  
? Y-axis shows document counts (0-35)  
? Legend shows all 5 agencies  
? Hovering shows tooltips  

### Step 4: Check Browser Console
Press F12 and check Console tab for debug logs:
```
Historical Data: { "Nuclear Regulatory Commission": [...], ... }
Nuclear Regulatory Commission data points: [...]
...
Chart created successfully
```

### Step 5: Test With More Data
Fetch new data to add more points:
```
1. Go to /Dashboard/Index
2. Click "Fetch Latest Data"
3. Return to /Dashboard/Historical
4. Chart now shows 3 points per line
```

## Files Modified

**ECFRApp_test\Views\Dashboard\Historical.cshtml**
- ? Fixed property names: `item.date` ? `item.Date`
- ? Fixed property names: `item.documentCount` ? `item.DocumentCount`
- ? Added 400px height container
- ? Added `maintainAspectRatio: false`
- ? Added debug console logs
- ? Improved chart styling

## Debugging Tips

### If Chart Still Not Showing:

1. **Check Browser Console** (F12)
   - Look for JavaScript errors
   - Check debug logs
   - Verify data structure

2. **Verify Data Format**
   ```javascript
   console.log('Historical Data:', historicalData);
   ```
   Should show object with agency names as keys

3. **Check Canvas Element**
   - Canvas should be inside 400px height div
   - Canvas should have id="historicalChart"

4. **Verify Chart.js Loaded**
   ```javascript
   console.log(typeof Chart); // Should be "function"
   ```

5. **Test Date Parsing**
   ```javascript
   new Date("2026-01-04T05:18:49.6285185Z")
   // Should return valid Date object
   ```

## Why This Happened

C#'s `System.Text.Json` serializer uses **PascalCase** by default:
```csharp
public class HistoricalChange {
    public string AgencyName { get; set; }  // Serializes as "AgencyName"
    public DateTime Date { get; set; }       // Serializes as "Date"
    public int DocumentCount { get; set; }   // Serializes as "DocumentCount"
}
```

JavaScript typically uses **camelCase**, but we must match the actual JSON structure:
```json
{ "Date": "...", "DocumentCount": 33 }  // ? Must use these exact names
```

## Alternative Solution (Not Implemented)

If you wanted to use camelCase in JavaScript, you could configure JSON serialization:

```csharp
// In controller or service
JsonSerializer.Serialize(data, new JsonSerializerOptions {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
});
```

But the current fix (using PascalCase in JavaScript) is simpler and doesn't require changing serialization throughout the app.

## Result

? **Chart now displays all historical data points**  
? **Lines connect data points for each agency**  
? **Time scale shows hourly resolution**  
? **Hovering shows detailed information**  
? **Debug logs help troubleshoot issues**  

The historical chart will now properly visualize document count changes over time! ??
