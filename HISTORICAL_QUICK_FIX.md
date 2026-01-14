# ?? Historical Data - Quick Fix Guide

## ?? Issue
Historical chart not showing document count changes effectively.

## ? Solution
1. Changed chart from **daily** to **hourly** time scale
2. Added **cleanup feature** to remove invalid data
3. Enhanced chart visibility and tooltips

## ?? To Fix Right Now

### Step 1: Restart Application
```
Stop app ? Restart app
```

### Step 2: Clean Up Invalid Data
```
1. Navigate to: /Dashboard/Historical
2. Click: "Cleanup Invalid Data" button
3. Confirm: Yes
4. Wait: Auto-refresh
```

### Step 3: View Clean Chart
```
? Chart now shows time-based trends
? Only valid agencies (5 agencies)
? No "Unknown", "1", "10" entries
```

## ?? Current Data

### Before Cleanup (10 entries):
```
? Unknown (2 entries)
? 1 (1 entry)
? 10 (1 entry)
? Unknown Agency (1 entry)
? Nuclear Regulatory Commission (33 docs)
? Administrative Committee of the Federal Register (27 docs)
? Miscellaneous Agencies (18 docs)
? National Capital Planning Commission (16 docs)
? Administrative Conference of the United States (6 docs)
```

### After Cleanup (5 entries):
```
? Nuclear Regulatory Commission (33 docs)
? Administrative Committee of the Federal Register (27 docs)
? Miscellaneous Agencies (18 docs)
? National Capital Planning Commission (16 docs)
? Administrative Conference of the United States (6 docs)
```

## ?? Chart Improvements

### Time Scale
**Before:** Daily (can't see multiple fetches same day)
**After:** Hourly (shows all fetches on same day)

### Display Format
```
Jan 4, 03:20  ? First fetch
Jan 4, 05:04  ? Latest fetch
```

### Visual Enhancements
- ? Larger points (easier to see)
- ? Better tooltips (date + time)
- ? Hover effects
- ? Color-coded lines per agency

## ?? Future Behavior

Every time you fetch new data:
```
Day 1 (Jan 4):  5 points (one per agency)
Day 2 (Jan 5):  10 points (5 agencies × 2 days)
Week 1:         35 points (5 agencies × 7 days)
```

Chart will show:
- Document count increases/decreases
- Trends over time
- Changes per agency

## ?? Quick Actions

### View Historical Data
```
URL: /Dashboard/Historical
```

### Cleanup Invalid Data
```
Click: "Cleanup Invalid Data" button (top right of chart card)
```

### API Access
```javascript
// Get historical data
GET /api/ECFRData/historical

// Cleanup invalid entries  
POST /api/ECFRData/cleanup-historical
```

## ? What's Fixed

? Chart shows hourly time resolution  
? Cleanup button removes invalid data  
? Better point visibility  
? Enhanced tooltips  
? Auto-refresh after cleanup  
? Confirmation before cleanup  

## ?? Files Modified

- `Views/Dashboard/Historical.cshtml` - Chart config + cleanup UI
- `Services/FileStorageService.cs` - Cleanup method
- `Controllers/ECFRDataController.cs` - Cleanup endpoint

## ?? Result

After cleanup, you'll have a **clean historical chart** showing only valid agencies with proper names, and the ability to track document count changes over time as you fetch data multiple times!
