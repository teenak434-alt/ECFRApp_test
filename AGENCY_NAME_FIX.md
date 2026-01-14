# Fix: Agency Names Should Not Be Numerical

## Problem
The `AgencyName` field was showing numerical values like "1", "10" instead of actual agency names. This was happening because the ECFR API returns CFR Title numbers in the hierarchy data.

## Solution Implemented

### Enhanced Agency Name Extraction Logic

Updated `ECFRDataService.cs` with the following improvements:

1. **Added Validation**: Check if extracted value is a number before using it as agency name
2. **Created Title Number Mapping**: Map CFR Title numbers to their actual agency names
3. **Multiple Fallback Strategies**: Try different properties in order of preference

### Changes Made

#### 1. Enhanced `ExtractAgencyName` Method
- Validates that agency strings are not just numbers
- Tries multiple property paths to find actual agency names
- Falls back to title number mapping if only numbers are found
- Returns "Unknown Agency" instead of numerical values

#### 2. Added `MapTitleNumberToAgency` Method
Maps CFR Title numbers to actual agencies:

| Title | Agency Name |
|-------|------------|
| 1 | General Provisions |
| 7 | Agriculture |
| 10 | Energy |
| 21 | Food and Drugs |
| 29 | Labor |
| 40 | Protection of Environment |
| 49 | Transportation |
| ... | (50+ mappings) |

### Expected Results After Restart

**Before Fix:**
```json
{
  "AgencyName": "1",
  "DocumentCount": 67
},
{
  "AgencyName": "10",
  "DocumentCount": 33
}
```

**After Fix:**
```json
{
  "AgencyName": "General Provisions",
  "DocumentCount": 67
},
{
  "AgencyName": "Energy",
  "DocumentCount": 33
}
```

## Testing After Restart

1. **Stop and restart the application** (required due to interface changes)

2. **Fetch new data**:
   - Navigate to `/Dashboard/Index`
   - Click "Fetch Latest Data"

3. **Verify agency names**:
   - Dashboard should show "General Provisions", "Energy", etc.
   - No numerical agency names like "1", "10"

4. **Check multiple views**:
   - `/Dashboard/Index` - Statistics table
   - `/DataViewer/Index` - Agency statistics section
   - `/Dashboard/Checksums` - Checksum table
   - All should show proper agency names

## Technical Details

### Property Search Order
1. `agency_names` (string)
2. `agencies` (string)
3. `agency` (string or object)
4. `hierarchy.title` (string, non-numeric)
5. `hierarchy.title_number` (mapped to name)
6. `agency.name` (from object)
7. `parent_agency` (string, non-numeric)

### Validation Logic
```csharp
// Rejects numerical values
if (!int.TryParse(agencyStr, out _))
{
    // Use this value - it's not just a number
}
else
{
    // Map the number to an agency name
    var mapped = MapTitleNumberToAgency(agencyStr);
}
```

## Files Modified

- `ECFRApp_test\Services\ECFRDataService.cs`
  - Enhanced `ExtractAgencyName()` method
  - Added `MapTitleNumberToAgency()` method

## Next Steps

1. **Restart the application**
2. **Fetch new data** to see proper agency names
3. **Old data** in `ecfr_current.json` will still have numbers until you fetch fresh data

## Notes

- The mapping covers all 50 CFR Titles
- If a new title number is added, it will show as "Title XX" format
- The fix ensures no numerical agency names appear in the UI
- Historical data will need to be re-fetched to update old numerical values
