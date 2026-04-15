# Driver Order Notification System

## Overview
The backend uses an **automatic dispatch system** where orders are assigned TO drivers via notifications, not fetched BY drivers.

## How It Works

### Backend Dispatch Flow
1. **Driver starts shift** → Calls `/DriverShift/start` → Enters queue
2. **Backend monitors queue** → Checks driver positions via `/DriverShift/queue-position`
3. **New order arrives** → Backend automatically assigns to next driver in queue
4. **Notification sent** → Driver receives push notification with `assignmentId`
5. **Driver responds** → Accepts via `/DriverDispatch/accept` OR Rejects via `/DriverDispatch/reject`
6. **Delivery tracking** → Completes via `/DriverDispatch/complete/{orderId}`

### Key Difference
- ❌ **OLD (Wrong)**: Driver polls `/Order` endpoint to fetch available orders
- ✅ **NEW (Correct)**: Driver receives notifications when orders are assigned automatically

## Current Implementation Status

### ✅ Fixed
- [x] `AvailableOrdersNotifier` no longer polls `/Order` endpoint
- [x] Added `addAssignment()` method to handle incoming notifications
- [x] Added `removeOrder()` method to clean up after accept/reject
- [x] Updated UI to show "Waiting for order assignments" message
- [x] Error handling for shift operations

### ⚠️ Needs Implementation
- [ ] **Push notification integration** (FCM/APNs)
- [ ] **Notification payload parsing** to extract `assignmentId`
- [ ] **Background notification handler** to receive assignments
- [ ] **Foreground notification handler** to update UI in real-time
- [ ] **Notification click handler** to open order details

## What Needs to Be Built

### 1. Push Notification Setup
```dart
// pubspec.yaml
dependencies:
  firebase_messaging: ^14.0.0
  flutter_local_notifications: ^16.0.0
```

### 2. Notification Handler
```dart
class DriverNotificationService {
  Future<void> initialize() async {
    // Initialize Firebase Messaging
    FirebaseMessaging messaging = FirebaseMessaging.instance;
    
    // Request permission
    await messaging.requestPermission();
    
    // Get FCM token and send to backend
    String? token = await messaging.getToken();
    // TODO: Send token to backend via /Driver/update-fcm-token
    
    // Handle foreground notifications
    FirebaseMessaging.onMessage.listen((RemoteMessage message) {
      _handleOrderAssignment(message);
    });
    
    // Handle background notifications
    FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);
  }
  
  void _handleOrderAssignment(RemoteMessage message) {
    // Parse notification data
    final data = message.data;
    final assignmentId = data['assignmentId'] as int?;
    final orderId = data['orderId'] as int?;
    
    if (assignmentId != null && orderId != null) {
      // Fetch order details from /Order/{orderId}
      // Create DriverOrder object
      // Call ref.read(availableOrdersNotifierProvider.notifier).addAssignment(order)
      
      // Show local notification
      _showLocalNotification(
        title: 'New Order Assignment',
        body: 'Order #$orderId has been assigned to you',
      );
    }
  }
}
```

### 3. Expected Notification Payload
The backend should send push notifications with this structure:
```json
{
  "notification": {
    "title": "New Order Assignment",
    "body": "You have been assigned order #12345"
  },
  "data": {
    "type": "order_assignment",
    "assignmentId": 789,
    "orderId": 12345,
    "pickupAddress": "...",
    "deliveryAddress": "...",
    "customerName": "..."
  }
}
```

### 4. Integration Points

#### When notification received:
```dart
// In DriverNotificationService
void _handleOrderAssignment(RemoteMessage message) {
  final data = message.data;
  
  // Create DriverOrder from notification data
  final order = DriverOrder(
    id: data['orderId'],
    assignmentId: data['assignmentId'],
    isAssigned: true,
    // ... other fields from notification
  );
  
  // Add to available orders
  ref.read(availableOrdersNotifierProvider.notifier).addAssignment(order);
}
```

#### When driver accepts:
```dart
// In AvailableOrdersNotifier
await acceptOrder(order.assignmentId!);

// This calls:
// POST /DriverDispatch/accept
// Body: { "assignmentId": 789 }

// Then remove from available orders
removeOrder(order.id);
```

#### When driver rejects:
```dart
// In AvailableOrdersNotifier
await rejectOrder(order.assignmentId!);

// This calls:
// POST /DriverDispatch/reject  
// Body: { "assignmentId": 789 }

// Then remove from available orders
removeOrder(order.id);
```

## Backend Requirements

### Endpoints Being Used
- ✅ `POST /DriverShift/start` - Start shift and join queue
- ✅ `POST /DriverShift/end` - End shift and leave queue
- ✅ `GET /DriverShift/current` - Get current shift status
- ✅ `GET /DriverShift/queue-position` - Get position in queue
- ✅ `POST /DriverDispatch/accept` - Accept order assignment
- ✅ `POST /DriverDispatch/reject` - Reject order assignment
- ✅ `POST /DriverDispatch/complete/{orderId}` - Mark delivery complete

### What Backend Needs to Send
When assigning an order to a driver, the backend should:
1. Create an assignment record with `assignmentId`
2. Send push notification to driver's FCM token with:
   - `assignmentId` (for accept/reject)
   - `orderId` (for order details)
   - Basic order info (pickup, delivery addresses)
3. Wait for driver response (accept/reject)
4. If accepted → move order to driver's active deliveries
5. If rejected → assign to next driver in queue

### Additional Endpoint Needed (Maybe)
- `POST /Driver/update-fcm-token` - Store driver's FCM token for push notifications
  - Request: `{ "fcmToken": "string" }`
  - Response: Success confirmation

## Testing Strategy

### 1. Test Shift Start
```bash
# Driver starts shift
POST /DriverShift/start
# Should return success and queue position
```

### 2. Simulate Notification
```bash
# Manually trigger a test notification from Firebase Console or backend
# Notification should contain assignmentId and orderId
```

### 3. Test Accept Flow
```bash
# After receiving notification:
POST /DriverDispatch/accept
Body: { "assignmentId": 789 }
# Should move order to active deliveries
```

### 4. Test Reject Flow
```bash
POST /DriverDispatch/reject
Body: { "assignmentId": 789 }
# Should remove order and assign to next driver
```

## Migration Path

### Phase 1: Remove Polling (✅ DONE)
- Stop calling `/Order` endpoint from `AvailableOrdersNotifier`
- Return empty list until notifications are ready
- Update UI to show "Waiting for assignments"

### Phase 2: Add Notification Infrastructure (⚠️ PENDING)
- Set up Firebase Cloud Messaging
- Implement notification handlers
- Parse notification payload

### Phase 3: Integrate with Existing Code (⚠️ PENDING)
- Call `addAssignment()` when notification received
- Test accept/reject flow
- Verify active deliveries show accepted orders

### Phase 4: Polish (⚠️ PENDING)
- Add notification sounds
- Handle edge cases (offline, app closed)
- Add retry logic for failed accept/reject

## Questions for Backend Team

1. **What is the notification payload structure?**
   - What fields are included in the push notification?
   - Is `assignmentId` always included?

2. **Do we need to register FCM tokens?**
   - Is there an endpoint to send the driver's FCM token?
   - How should we handle token refresh?

3. **What happens if driver doesn't respond?**
   - Auto-reject after timeout?
   - Notification retry?

4. **Can we test notifications in development?**
   - Is there a test endpoint to trigger assignment?
   - Can we use Firebase Console for testing?

5. **Shift start 400 error - what's the validation?**
   - What causes "One or more validation errors occurred"?
   - Does driver need to end previous shift first?
   - Are there location requirements?

## Notes
- The `/Order` endpoint is for **tenants only** to manage orders
- Drivers should **never** call `/Order` to fetch orders
- All driver-order interactions go through `/DriverDispatch/*` endpoints
- The backend handles queue management and automatic assignment
