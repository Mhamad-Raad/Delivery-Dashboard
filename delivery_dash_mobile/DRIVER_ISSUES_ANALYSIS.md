# ⚠️ OBSOLETE - Driver Feature Issues & Fixes

**Date:** January 18, 2026  
**Status:** ⚠️⚠️⚠️ **OBSOLETE - SUPERSEDED BY DRIVER_ORDER_NOTIFICATION_SYSTEM.md** ⚠️⚠️⚠️

---

## ⚠️ THIS DOCUMENT IS NO LONGER ACCURATE ⚠️

This analysis was based on the **incorrect assumption** that drivers need to fetch orders via API endpoints.

**ACTUAL BACKEND BEHAVIOR:**
- Backend **automatically assigns** orders to drivers in the queue
- Orders are **pushed** to drivers via **notifications**, not fetched
- No "available orders" endpoint needed - backend handles assignment
- See [DRIVER_ORDER_NOTIFICATION_SYSTEM.md](DRIVER_ORDER_NOTIFICATION_SYSTEM.md) for correct architecture

**Why This Document Was Wrong:**
- Assumed drivers need `/DriverDispatch/available-orders` endpoint ❌
- Assumed drivers need `/DriverDispatch/my-assignments` endpoint ❌  
- Assumed polling/fetching model instead of push notification model ❌

**Correct Information:**
- See [DRIVER_ORDER_NOTIFICATION_SYSTEM.md](DRIVER_ORDER_NOTIFICATION_SYSTEM.md)
- See [DRIVER_ISSUES_SUMMARY.md](DRIVER_ISSUES_SUMMARY.md)

---

## Original (Incorrect) Analysis Below

### 1. ❌ CRITICAL: Missing Driver-Specific Order Endpoints

**Problem:** The backend API is **missing endpoints** for drivers to retrieve orders.

**Current Situation:**
- The app uses `/MalDashApi/Order` with status filters to get driver orders
- This endpoint is designed for **tenants**, not drivers
- Backend likely returns 401/403 because drivers shouldn't access tenant endpoints
- **No way for drivers to see available or assigned orders!**

**What's Missing from Backend:**
```
❌ GET /MalDashApi/DriverDispatch/available-orders
   → Get orders ready to be assigned to drivers (status 4)
   
❌ GET /MalDashApi/DriverDispatch/my-assignments
   → Get orders currently assigned to this driver (status 5)
   
❌ GET /MalDashApi/DriverDispatch/my-deliveries  
   → Get orders being delivered by this driver (status 6, 7)
```

**Current Workaround in App:**
```dart
// ❌ WRONG: Using tenant endpoint
final readyOrders = await repository.getOrders(
  status: '4', // Ready for pickup
);

// ✅ CORRECT: Should use driver endpoint (which doesn't exist!)
final availableOrders = await driverDispatchRepository.getAvailableOrders();
```

---

### 2. ⚠️ Shift Start Returns 400 Error

**Problem:** `POST /MalDashApi/DriverShift/start` returns HTTP 400 error.

**Possible Causes:**
1. **User already has an active shift** - Backend validation error
2. **Role validation failure** - User role is not recognized as driver
3. **Missing required headers** - Authorization or content-type issues
4. **Database constraint** - Foreign key or data integrity issue

**What the App Sends:**
```dart
POST /MalDashApi/DriverShift/start
Headers: {
  'Authorization': 'Bearer <token>',
  'Content-Type': 'application/json'
}
Body: (empty - per API spec)
```

**Expected Response:**
```json
200 OK
{
  "isActive": true,
  "startTime": "2026-01-18T10:30:00Z",
  ...
}
```

**Actual Response:**
```
400 Bad Request
Error: DioException [bad response]: This exception was thrown because the response has a status code of 400 and RequestOptions.validateStatus was configured to throw for this status code.
```

---

## Backend API Requirements

### Required Endpoints to Add

#### 1. Get Available Orders for Assignment
```
GET /MalDashApi/DriverDispatch/available-orders
Query Parameters:
  - page: int (default: 1)
  - limit: int (default: 10)
  
Response: 200 OK
[
  {
    "id": 123,
    "orderNumber": "ORD-2026-001",
    "status": 4,
    "vendorName": "Pizza Palace",
    "deliveryAddress": "123 Main St, Apt 5B",
    "totalAmount": 45.50,
    "deliveryFee": 5.00,
    "createdAt": "2026-01-18T10:00:00Z",
    "items": [...]
  }
]
```

#### 2. Get My Assigned Orders
```
GET /MalDashApi/DriverDispatch/my-assignments
Query Parameters:
  - page: int (default: 1)
  - limit: int (default: 10)
  
Response: 200 OK
[
  {
    "assignmentId": 456,
    "orderId": 123,
    "assignedAt": "2026-01-18T10:15:00Z",
    "order": { ... }
  }
]
```

#### 3. Get My Active Deliveries
```
GET /MalDashApi/DriverDispatch/my-deliveries
Query Parameters:
  - page: int (default: 1)
  - limit: int (default: 10)
  - status: string (optional: "6" for picked-up, "7" for in-transit)
  
Response: 200 OK
[
  {
    "id": 123,
    "status": 6,
    "pickedUpAt": "2026-01-18T10:20:00Z",
    ...
  }
]
```

---

### Fix Shift Start Endpoint

**Diagnosis Steps:**

1. **Check Backend Logs** for the 400 error:
   ```
   - What validation is failing?
   - Is there an error message?
   - Stack trace?
   ```

2. **Verify User Role** in database:
   ```sql
   SELECT Id, Email, Role FROM Users WHERE Email = 'driver@example.com';
   -- Should return Role = 4 or 'Driver'
   ```

3. **Check for Existing Shift**:
   ```sql
   SELECT * FROM DriverShifts 
   WHERE DriverId = '<user-id>' 
   AND IsActive = 1;
   -- If exists, need to end it first
   ```

4. **Test with API Client** (Postman/Insomnia):
   ```
   POST https://maldash-development-api.runasp.net/MalDashApi/DriverShift/start
   Headers:
     Authorization: Bearer <driver-token>
   Body: (empty)
   ```

---

## App-Side Temporary Fixes

### Fix 1: Add Error Handling for Shift Start

**File:** `lib/src/features/driver/data/driver_shift_repository.dart`

```dart
Future<DriverShift?> startShift() async {
  try {
    print('Starting driver shift...');

    final response = await _dio.post('/DriverShift/start');

    print('Start shift response status: ${response.statusCode}');
    print('Start shift response data: ${response.data}');

    if (response.statusCode == 200) {
      if (response.data != null && response.data is Map<String, dynamic>) {
        return DriverShift.fromJson(response.data);
      }
      return DriverShift(isActive: true, startTime: DateTime.now());
    }
  } on DioException catch (e) {
    print('DioException starting shift: ${e.response?.statusCode}');
    print('Error response: ${e.response?.data}');
    
    if (e.response?.statusCode == 400) {
      // Parse error message from backend
      final errorData = e.response?.data;
      String errorMessage = 'Failed to start shift';
      
      if (errorData is Map && errorData.containsKey('message')) {
        errorMessage = errorData['message'];
      } else if (errorData is String) {
        errorMessage = errorData;
      }
      
      throw Exception('Shift Start Error: $errorMessage');
    }
    
    rethrow;
  } catch (e) {
    print('Error starting shift: $e');
    rethrow;
  }
  
  return null;
}
```

### Fix 2: Comment Out Order Fetching Until Backend is Ready

**File:** `lib/src/features/driver/presentation/driver_orders_notifier.dart`

```dart
Future<List<DriverOrder>> _loadAvailableOrders() async {
  final repository = ref.read(driverOrdersRepositoryProvider);

  // ⚠️ TEMPORARY: Backend missing driver-specific order endpoints
  // Using tenant /Order endpoint will fail with 403/401
  // TODO: Replace with /DriverDispatch/available-orders when ready
  
  try {
    final readyOrders = await repository.getOrders(
      page: 1,
      limit: 50,
      status: '4',
    );
    final assignedOrders = await repository.getOrders(
      page: 1,
      limit: 50,
      status: '5',
    );

    final allOrders = [...readyOrders, ...assignedOrders];
    // ... rest of code
    
    return uniqueOrders.values.toList();
  } catch (e) {
    print('⚠️ Cannot fetch driver orders - backend endpoint missing');
    print('Error: $e');
    // Return empty list until backend adds driver endpoints
    return [];
  }
}
```

---

## Backend Implementation Guide (for Backend Team)

### 1. Create DriverDispatchController Methods

```csharp
[ApiController]
[Route("MalDashApi/[controller]")]
[Authorize(Roles = "Driver")] // Only drivers can access
public class DriverDispatchController : ControllerBase
{
    [HttpGet("available-orders")]
    public async Task<IActionResult> GetAvailableOrders(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Get orders with status 4 (Ready for Pickup)
        // that are NOT already assigned to another driver
        var orders = await _context.Orders
            .Where(o => o.Status == OrderStatus.ReadyForPickup)
            .Where(o => !_context.OrderAssignments.Any(a => 
                a.OrderId == o.Id && 
                a.Status == AssignmentStatus.Active))
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
            
        return Ok(orders);
    }
    
    [HttpGet("my-assignments")]
    public async Task<IActionResult> GetMyAssignments(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var assignments = await _context.OrderAssignments
            .Include(a => a.Order)
            .Where(a => a.DriverId == userId)
            .Where(a => a.Status == AssignmentStatus.Active)
            .OrderByDescending(a => a.AssignedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
            
        return Ok(assignments);
    }
    
    [HttpGet("my-deliveries")]
    public async Task<IActionResult> GetMyDeliveries(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? status = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var query = _context.Orders
            .Where(o => o.DriverId == userId)
            .Where(o => o.Status == OrderStatus.PickedUp || 
                       o.Status == OrderStatus.InTransit);
        
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }
        }
        
        var deliveries = await query
            .OrderByDescending(o => o.PickedUpAt ?? o.UpdatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
            
        return Ok(deliveries);
    }
}
```

### 2. Fix DriverShiftController Start Method

```csharp
[HttpPost("start")]
public async Task<IActionResult> StartShift()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    // Check if driver already has an active shift
    var existingShift = await _context.DriverShifts
        .Where(s => s.DriverId == userId && s.IsActive)
        .FirstOrDefaultAsync();
        
    if (existingShift != null)
    {
        // Return error with clear message
        return BadRequest(new { 
            message = "You already have an active shift. Please end it before starting a new one.",
            currentShift = existingShift
        });
    }
    
    // Create new shift
    var shift = new DriverShift
    {
        DriverId = userId,
        StartTime = DateTime.UtcNow,
        IsActive = true
    };
    
    _context.DriverShifts.Add(shift);
    await _context.SaveChangesAsync();
    
    return Ok(shift);
}
```

---

## Testing Checklist

### Backend Tests (API Team)

- [ ] Test `/DriverShift/start` with driver token
- [ ] Test `/DriverShift/start` with existing active shift (should return 400)
- [ ] Test `/DriverShift/end` successfully
- [ ] Add `/DriverDispatch/available-orders` endpoint
- [ ] Add `/DriverDispatch/my-assignments` endpoint
- [ ] Add `/DriverDispatch/my-deliveries` endpoint
- [ ] Test all new endpoints return proper data
- [ ] Test endpoints reject non-driver roles (401/403)

### App Tests (After Backend is Fixed)

- [ ] Driver can start shift successfully
- [ ] Shift start shows proper error message if already active
- [ ] Driver can see available orders
- [ ] Driver can see assigned orders
- [ ] Driver can accept an order
- [ ] Driver can reject an order
- [ ] Driver can see active deliveries
- [ ] Driver can complete a delivery

---

## Summary

**Critical Issues:**
1. ❌ **Backend missing 3 critical driver endpoints** - Cannot fetch orders
2. ⚠️ **Shift start returning 400** - Likely validation error

**Actions Required:**

**Backend Team:**
1. Add `/DriverDispatch/available-orders` endpoint
2. Add `/DriverDispatch/my-assignments` endpoint  
3. Add `/DriverDispatch/my-deliveries` endpoint
4. Fix `/DriverShift/start` to return proper error messages
5. Check why shift start is failing (likely existing shift check)

**App Team (Temporary):**
1. Add better error handling for shift start
2. Show empty state for orders until backend is ready
3. Add user-friendly error messages

**Priority:** 🔴 **CRITICAL** - Drivers cannot use the app until backend endpoints are added!
