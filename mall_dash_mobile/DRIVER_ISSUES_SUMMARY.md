# Driver Issues - Fixed and Pending

## Summary
This document tracks the resolution of driver-related issues in the Delivery Dash Mobile app.

## Issues Reported

### 1. ✅ FIXED: Role Conflict - Driver Seeing Tenant Pages
**Problem**: After login, driver users (role "4") were seeing tenant pages/UI instead of driver pages.

**Root Cause**: The `userProfileProvider` was not being invalidated on logout/login, causing cached profile data from previous user to persist.

**Fix Applied**: 
- Modified [auth_notifier.dart](lib/src/features/auth/presentation/auth_notifier.dart) `logout()` method to invalidate all user-specific providers
- Modified [auth_notifier.dart](lib/src/features/auth/presentation/auth_notifier.dart) `setAuthenticated()` to invalidate `userProfileProvider` forcing fresh fetch
- Added comprehensive logging in [main_scaffold.dart](lib/src/core/presentation/main_scaffold.dart) for role detection

**Files Modified**:
- [lib/src/features/auth/presentation/auth_notifier.dart](lib/src/features/auth/presentation/auth_notifier.dart#L120-L140)
- [lib/src/core/presentation/main_scaffold.dart](lib/src/core/presentation/main_scaffold.dart)
- [lib/src/features/profile/data/user_profile_repository.dart](lib/src/features/profile/data/user_profile_repository.dart)

**Documentation**: See [ROLE_CONFLICT_FIX.md](ROLE_CONFLICT_FIX.md)

---

### 2. ⚠️ PENDING: Shift Start Returns 400 Error
**Problem**: When driver tries to start shift, backend returns 400 "One or more validation errors occurred"

**Current Status**: Enhanced error logging to capture detailed validation errors

**Enhanced Logging**:
- Added detailed request/response logging in [driver_shift_repository.dart](lib/src/features/driver/data/driver_shift_repository.dart)
- Captures: status code, error data structure, validation error details
- Added 409 conflict handling for "already has active shift"

**Next Steps**:
1. Test with actual driver account credentials
2. Check backend logs to see what validation is failing
3. Possible causes:
   - Driver already has an active shift (need to end it first)
   - Missing required profile data (location, vehicle info, etc.)
   - Backend permission check failing
   - Token/authentication issue

**Files Modified**:
- [lib/src/features/driver/data/driver_shift_repository.dart](lib/src/features/driver/data/driver_shift_repository.dart#L10-L65)

---

### 3. ⚠️ REDESIGNED: Orders Not Showing After Assignment
**Problem**: Driver doesn't see orders when they are assigned

**Original Assumption**: ❌ Backend missing driver order endpoints  
**Backend Clarification**: ✅ Backend automatically assigns orders via **notifications**, not manual fetching

**How It Actually Works**:
1. Driver starts shift → enters queue at position N
2. New order arrives → **backend automatically assigns** to next driver in queue
3. Driver receives **push notification** with `assignmentId` and `orderId`
4. Driver accepts via `/DriverDispatch/accept` or rejects via `/DriverDispatch/reject`
5. Accepted order moves to active deliveries
6. Driver completes via `/DriverDispatch/complete/{orderId}`

**Wrong Approach (Removed)**:
- ❌ Trying to fetch orders from `/Order` endpoint (tenant-only)
- ❌ Polling for "available orders" with status filters
- ❌ Trying to fetch "active deliveries" from `/Order` endpoint

**Correct Approach (Implemented)**:
- ✅ Wait for notification from backend
- ✅ Parse `assignmentId` from notification payload
- ✅ Use `addAssignment()` method to add order to UI
- ✅ Use existing accept/reject/complete methods
- ✅ **Client-side tracking of active deliveries** until backend adds `/DriverDispatch/my-active-orders`

**Temporary Workaround**:
Since drivers can't access `/Order` endpoint (returns 400), we're now:
1. Returning empty list for available orders (waiting for notifications)
2. Returning empty list for active deliveries (no backend endpoint exists)
3. When driver **accepts** an order, we add it to active deliveries client-side
4. When driver **completes** a delivery, we remove it from active deliveries client-side
5. This works until backend provides `/DriverDispatch/my-active-orders` endpoint

**Files Modified**:
- [lib/src/features/driver/presentation/driver_orders_notifier.dart](lib/src/features/driver/presentation/driver_orders_notifier.dart)
  - Removed `/Order` endpoint polling
  - Added `addAssignment()` method for incoming notifications
  - Added `removeOrder()` method for cleanup after accept/reject
  - Enhanced error handling for active deliveries
- [lib/src/features/driver/presentation/available_orders_page.dart](lib/src/features/driver/presentation/available_orders_page.dart)
  - Updated empty state message to show "Waiting for order assignments"
  - Changed icon from shopping bag to notification bell
  - Added explanation that orders are automatically assigned

**Documentation**: See [DRIVER_ORDER_NOTIFICATION_SYSTEM.md](DRIVER_ORDER_NOTIFICATION_SYSTEM.md)

---

## Backend Endpoints Being Used

### Driver Shift Management
- ✅ `POST /DriverShift/start` - Start shift and join queue
- ✅ `POST /DriverShift/end` - End shift and leave queue  
- ✅ `GET /DriverShift/current` - Get current shift status
- ✅ `GET /DriverShift/queue-position` - Get position in queue

### Driver Dispatch (Order Assignment)
- ✅ `POST /DriverDispatch/accept` - Accept order assignment
  - Body: `{ "assignmentId": number }`
- ✅ `POST /DriverDispatch/reject` - Reject order assignment
  - Body: `{ "assignmentId": number }`
- ✅ `POST /DriverDispatch/complete/{orderId}` - Mark delivery complete

### Order Details
- ✅ `GET /Order/{id}` - Get order details (after accepting assignment)

### ❌ Missing Backend Endpoints
- ❌ `GET /DriverDispatch/my-active-orders` - Get driver's current active deliveries
  - **Workaround**: Client-side tracking of accepted orders until this endpoint is added
  - When driver accepts order → add to local state
  - When driver completes order → remove from local state

---

## What Still Needs to Be Built

### Push Notification Integration
The app currently has NO push notification system. Need to implement:

1. **Firebase Cloud Messaging Setup**
   ```yaml
   dependencies:
     firebase_messaging: ^14.0.0
     flutter_local_notifications: ^16.0.0
   ```

2. **Notification Service**
   - Initialize FCM
   - Request notification permissions
   - Get and store FCM token
   - Send FCM token to backend (need endpoint: `POST /Driver/update-fcm-token`)

3. **Notification Handlers**
   - Foreground: Show in-app notification + update UI
   - Background: Show system notification
   - Notification click: Open order details page

4. **Integration with Existing Code**
   ```dart
   // When notification received with order assignment
   void _handleOrderAssignment(RemoteMessage message) {
     final assignmentId = message.data['assignmentId'];
     final orderId = message.data['orderId'];
     
     // Fetch full order details
     final order = await orderRepository.getOrder(orderId);
     
     // Add to available orders
     ref.read(availableOrdersNotifierProvider.notifier)
        .addAssignment(order);
   }
   ```

---

## Questions for Backend Team

### About Notifications
1. What is the notification payload structure?
   - What fields are included in the push notification data?
   - Is `assignmentId` always included?
   - Can we get sample payload?

2. How do we register FCM tokens?
   - Is there a `POST /Driver/update-fcm-token` endpoint?
   - When should we send the token (login, shift start)?

3. What happens if driver doesn't respond to assignment?
   - Auto-reject after timeout?
   - Re-notification?
   - Assign to next driver?

### About Shift Start Error
4. What validation is causing the 400 error on shift start?
   - "One or more validation errors occurred" - what exactly?
   - Does driver need to end previous shift first?
   - Are there profile requirements (location, vehicle, etc.)?
   - Can we get detailed error message?

### About Order Assignment Flow
5. When exactly does backend assign orders?
   - Immediately when order is created?
   - Based on queue position?
   - Geographic proximity?

6. Can drivers see queue position in real-time?
   - Should we poll `/DriverShift/queue-position`?
   - Or is it sent in notifications?

---

## Testing Checklist

### Role-Based Navigation (✅ TESTED)
- [x] Login as driver → should see driver dashboard
- [x] Login as tenant → should see tenant dashboard
- [x] Logout and login as different role → UI should update
- [x] No cached data from previous user

### Shift Management (⚠️ NEEDS TESTING)
- [ ] Start shift → should return success or detailed error
- [ ] Get current shift status → should show active/inactive
- [ ] Get queue position → should return position number
- [ ] End shift → should return success

### Order Assignment (⚠️ PENDING NOTIFICATION SYSTEM)
- [ ] Start shift → join queue
- [ ] Backend assigns order → receive notification
- [ ] Notification shows correct order details
- [ ] Accept order → moves to active deliveries
- [ ] Reject order → removed from available orders
- [ ] Complete delivery → removed from active deliveries

### Edge Cases (⚠️ PENDING)
- [ ] App in background when notification arrives
- [ ] App closed when notification arrives
- [ ] No internet connection during accept/reject
- [ ] Driver already has active shift
- [ ] Invalid token/authentication

---

## Architecture Changes

### Before (Wrong)
```
Driver App → Poll /Order endpoint → Get orders with status filters → Show in UI
```

### After (Correct)
```
Backend → Assign order to driver → Send push notification → 
Driver App receives notification → Parse assignmentId → 
Add to UI via addAssignment() → 
Driver accepts/rejects via /DriverDispatch
```

---

## Implementation Priority

### High Priority (Blocking)
1. ⚠️ **Fix shift start 400 error** - Can't test anything without starting shift
2. ⚠️ **Implement push notifications** - Required for order assignment system

### Medium Priority (Important but not blocking)
3. Add FCM token registration endpoint
4. Add notification sound/vibration
5. Add order assignment timeout handling

### Low Priority (Nice to have)
6. Real-time queue position updates
7. Notification history/logs
8. Offline mode handling

---

## Files Summary

### Modified Files
- [lib/src/features/auth/presentation/auth_notifier.dart](lib/src/features/auth/presentation/auth_notifier.dart) - Fixed provider invalidation
- [lib/src/features/driver/data/driver_shift_repository.dart](lib/src/features/driver/data/driver_shift_repository.dart) - Enhanced error logging
- [lib/src/features/driver/presentation/driver_orders_notifier.dart](lib/src/features/driver/presentation/driver_orders_notifier.dart) - Redesigned for notifications
- [lib/src/features/driver/presentation/available_orders_page.dart](lib/src/features/driver/presentation/available_orders_page.dart) - Updated UI messages

### Documentation Files
- [ROLE_CONFLICT_FIX.md](ROLE_CONFLICT_FIX.md) - Role caching bug analysis
- [DRIVER_ORDER_NOTIFICATION_SYSTEM.md](DRIVER_ORDER_NOTIFICATION_SYSTEM.md) - Notification system architecture
- [DRIVER_ISSUES_ANALYSIS.md](DRIVER_ISSUES_ANALYSIS.md) - ⚠️ OBSOLETE (was based on wrong assumption)

---

## Next Steps

1. **Test shift start with driver account**
   - Login with actual driver credentials
   - Try starting shift
   - Capture full error details from enhanced logging

2. **Coordinate with backend team**
   - Get notification payload structure
   - Get FCM token registration endpoint
   - Understand shift start validation

3. **Implement notification system**
   - Set up Firebase Cloud Messaging
   - Create notification service
   - Integrate with existing order flow

4. **Test end-to-end flow**
   - Start shift → receive assignment → accept → complete
   - Verify all data flows correctly
   - Handle edge cases
