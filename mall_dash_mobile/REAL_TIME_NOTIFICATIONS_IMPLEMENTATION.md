# Real-Time Driver Notifications - Implementation Plan

## Current Status Analysis

### ❌ App Side - NO Real-Time Notifications
- **No Firebase Cloud Messaging** - `pubspec.yaml` doesn't have `firebase_messaging` package
- **No WebSocket/SignalR** - No real-time connection implementation found
- **Basic Notification Page** - Only has a placeholder UI, no actual notification handling
- **No FCM Configuration** - Missing `google-services.json` (Android) and `GoogleService-Info.plist` (iOS)

### ⚠️ Backend Side - Polling-Based Notifications
- **Has Notification Endpoints** (polling-based):
  - `GET /MalDashApi/Notification` - Get list of notifications (skip/take pagination)
  - `GET /MalDashApi/Notification/unread-count` - Get count of unread notifications
  - `GET /MalDashApi/Notification/{id}` - Get specific notification
  - `POST /MalDashApi/Notification/{id}/read` - Mark notification as read
  - `POST /MalDashApi/Notification/mark-all-read` - Mark all as read

- **NO Real-Time Push** - No endpoints for:
  - FCM token registration
  - WebSocket/SignalR hub
  - Server-Sent Events (SSE)

## Problem: Not Real-Time!

The current backend only supports **polling** (app asks "any new notifications?" every X seconds). This is:
- ❌ **Battery drain** - Constant HTTP requests
- ❌ **Delayed** - Driver might not see order assignment for 10-30 seconds
- ❌ **Data waste** - Multiple requests even when no new notifications
- ❌ **Not scalable** - 1000 drivers = 1000 requests every 10 seconds

## Solution Options

### Option 1: Firebase Cloud Messaging (FCM) ⭐ RECOMMENDED
**Best for:** Mobile apps with order assignments

**How it works:**
1. App gets FCM token from Google
2. App sends token to backend via new endpoint
3. When backend assigns order → sends push notification to driver's FCM token
4. Driver receives notification instantly (even if app is closed)
5. App handles notification → shows order → driver accepts/rejects

**Pros:**
- ✅ True real-time (< 1 second delay)
- ✅ Works when app is closed/background
- ✅ Reliable (Google infrastructure)
- ✅ Free for unlimited notifications
- ✅ Cross-platform (iOS/Android/Web)

**Cons:**
- ⚠️ Requires Firebase project setup
- ⚠️ Requires backend to integrate Firebase Admin SDK
- ⚠️ Need to handle iOS APNs certificates

### Option 2: SignalR WebSocket
**Best for:** Web apps or real-time chat

**How it works:**
1. App opens WebSocket connection to backend SignalR hub
2. Backend sends messages through the open connection
3. App receives messages instantly

**Pros:**
- ✅ Bidirectional communication
- ✅ No third-party service needed

**Cons:**
- ❌ Connection dies when app is closed/background
- ❌ Battery drain (constant connection)
- ❌ Complex to implement on backend (.NET SignalR)
- ❌ Less reliable on mobile networks

### Option 3: Polling with Optimization
**Best for:** Quick temporary solution

**How it works:**
1. App polls `/Notification` endpoint every 15-30 seconds
2. When driver is on shift → poll every 5 seconds
3. Use `unread-count` first, only fetch full list if count > 0

**Pros:**
- ✅ Works with current backend
- ✅ Easy to implement
- ✅ No backend changes needed

**Cons:**
- ❌ Not truly real-time (5-30 second delay)
- ❌ Battery drain
- ❌ Wasted requests

## Recommended Approach: FCM + Polling Fallback

### Phase 1: Implement Polling (Quick Win - 2 hours)
Get basic notifications working NOW while FCM is being set up.

**App Changes:**
1. Add notification polling service
2. Poll every 10 seconds when driver is on shift
3. Show notifications in UI
4. Handle order assignment notifications

**No Backend Changes Needed**

### Phase 2: Implement FCM (Proper Solution - 1-2 days)
Add real-time push notifications for production use.

**App Changes:**
1. Add `firebase_messaging` package
2. Set up Firebase project
3. Get FCM token on app start
4. Send token to backend
5. Handle incoming push notifications
6. Stop polling when FCM is active

**Backend Changes Required:**
1. Add Firebase Admin SDK
2. Add endpoint: `POST /Driver/update-fcm-token`
3. When assigning order → send FCM push notification
4. Include `assignmentId` and `orderId` in notification data

## Implementation Plan

### Phase 1: Polling-Based Notifications (IMMEDIATE)

#### Step 1: Add Notification Models
```dart
// lib/src/features/notifications/data/notification_model.dart
class AppNotification {
  final int id;
  final String title;
  final String message;
  final String type; // 'order_assignment', 'order_cancelled', etc.
  final Map<String, dynamic>? data; // Contains assignmentId, orderId
  final bool isRead;
  final DateTime createdAt;
}
```

#### Step 2: Add Notification Repository
```dart
// lib/src/features/notifications/data/notification_repository.dart
class NotificationRepository {
  Future<List<AppNotification>> getNotifications({int skip = 0, int take = 20});
  Future<int> getUnreadCount();
  Future<void> markAsRead(int notificationId);
  Future<void> markAllAsRead();
}
```

#### Step 3: Add Notification Polling Service
```dart
// lib/src/features/driver/data/driver_notification_service.dart
class DriverNotificationService {
  Timer? _pollingTimer;
  
  void startPolling() {
    _pollingTimer = Timer.periodic(Duration(seconds: 10), (_) {
      _checkForNewNotifications();
    });
  }
  
  void stopPolling() {
    _pollingTimer?.cancel();
  }
  
  Future<void> _checkForNewNotifications() async {
    final unreadCount = await notificationRepository.getUnreadCount();
    if (unreadCount > 0) {
      final notifications = await notificationRepository.getNotifications(take: unreadCount);
      _handleNotifications(notifications);
    }
  }
  
  void _handleNotifications(List<AppNotification> notifications) {
    for (final notification in notifications) {
      if (notification.type == 'order_assignment') {
        final assignmentId = notification.data?['assignmentId'];
        final orderId = notification.data?['orderId'];
        // Fetch order details and add to available orders
        _handleOrderAssignment(assignmentId, orderId);
      }
    }
  }
}
```

#### Step 4: Integrate with Driver Shift
```dart
// Start polling when shift starts
await driverNotificationService.startPolling();

// Stop polling when shift ends
await driverNotificationService.stopPolling();
```

### Phase 2: FCM Implementation (PROPER SOLUTION)

#### Step 1: Add Dependencies
```yaml
# pubspec.yaml
dependencies:
  firebase_core: ^3.10.0
  firebase_messaging: ^15.1.5
  flutter_local_notifications: ^18.0.1
```

#### Step 2: Firebase Project Setup
1. Create Firebase project at console.firebase.google.com
2. Add Android app (package: `com.example.delivery_dash_mobile`)
3. Download `google-services.json` → `android/app/`
4. Add iOS app
5. Download `GoogleService-Info.plist` → `ios/Runner/`

#### Step 3: Android Configuration
```gradle
// android/app/build.gradle
dependencies {
    implementation platform('com.google.firebase:firebase-bom:33.0.0')
    implementation 'com.google.firebase:firebase-messaging'
}
```

#### Step 4: iOS Configuration
```xml
<!-- ios/Runner/Info.plist -->
<key>FirebaseAppDelegateProxyEnabled</key>
<false/>
```

#### Step 5: Implement FCM Service
```dart
// lib/src/core/services/fcm_service.dart
class FCMService {
  final FirebaseMessaging _messaging = FirebaseMessaging.instance;
  
  Future<void> initialize() async {
    // Request permission
    await _messaging.requestPermission();
    
    // Get FCM token
    final token = await _messaging.getToken();
    
    // Send to backend
    await _sendTokenToBackend(token);
    
    // Handle token refresh
    _messaging.onTokenRefresh.listen(_sendTokenToBackend);
    
    // Handle foreground messages
    FirebaseMessaging.onMessage.listen(_handleForegroundMessage);
    
    // Handle background messages
    FirebaseMessaging.onBackgroundMessage(_handleBackgroundMessage);
  }
  
  Future<void> _sendTokenToBackend(String? token) async {
    if (token != null) {
      // POST /Driver/update-fcm-token
      await driverRepository.updateFCMToken(token);
    }
  }
  
  void _handleForegroundMessage(RemoteMessage message) {
    final data = message.data;
    if (data['type'] == 'order_assignment') {
      final assignmentId = int.parse(data['assignmentId']);
      final orderId = int.parse(data['orderId']);
      _handleOrderAssignment(assignmentId, orderId);
    }
  }
}
```

#### Step 6: Backend Integration Points

**New Endpoint Needed:**
```csharp
// POST /MalDashApi/Driver/update-fcm-token
public async Task<IActionResult> UpdateFCMToken([FromBody] UpdateFCMTokenRequest request)
{
    var driverId = User.GetUserId();
    await _driverRepository.UpdateFCMToken(driverId, request.FcmToken);
    return Ok();
}
```

**Send Notification When Assigning Order:**
```csharp
// In order assignment logic
var driver = await _driverRepository.GetNextDriverInQueue();
var assignment = await _assignmentRepository.Create(orderId, driver.Id);

// Send FCM notification
await _fcmService.SendNotification(driver.FcmToken, new {
    title = "New Order Assignment",
    body = $"Order #{order.OrderNumber} has been assigned to you",
    data = new {
        type = "order_assignment",
        assignmentId = assignment.Id,
        orderId = order.Id
    }
});
```

## Testing Strategy

### Phase 1 Testing (Polling)
1. Start driver shift
2. Backend assigns order
3. Wait max 10 seconds → notification should appear
4. Click notification → order details shown
5. Accept order → moves to active deliveries

### Phase 2 Testing (FCM)
1. Check FCM token saved to backend
2. Kill app completely
3. Backend assigns order
4. Push notification appears on device (< 2 seconds)
5. Tap notification → app opens to order details
6. Accept order → works correctly

## Questions for Backend Team

### Critical Questions:
1. **What notification format does backend send?**
   - What fields are in the notification object?
   - Does it include `assignmentId` and `orderId`?
   - Example JSON response?

2. **Can backend add FCM support?**
   - Can you integrate Firebase Admin SDK?
   - Can you add `/Driver/update-fcm-token` endpoint?
   - Can you send push notifications when assigning orders?

3. **What triggers a notification?**
   - Only order assignments?
   - Order cancellations?
   - Customer messages?

### Nice-to-Have Questions:
4. **Do notifications persist in database?**
   - Can driver see notification history?
   - How long are they stored?

5. **Can we filter notifications by type?**
   - `GET /Notification?type=order_assignment`

## Timeline Estimate

### Phase 1: Polling (Temporary Solution)
- **Time:** 4-6 hours
- **App Work:** 4 hours (models, repository, polling service, UI integration)
- **Backend Work:** 0 hours (uses existing endpoints)
- **Testing:** 1 hour

### Phase 2: FCM (Production Solution)
- **Time:** 2-3 days
- **App Work:** 1 day (Firebase setup, FCM service, notification handling)
- **Backend Work:** 1 day (Firebase Admin SDK, token endpoint, send notifications)
- **Testing:** 4 hours (iOS + Android + background scenarios)

## Recommendation

**START WITH PHASE 1 TODAY**
- Get notifications working in 4-6 hours using polling
- Driver can receive order assignments (with 10-second delay)
- No backend changes required
- Can go to production quickly

**IMPLEMENT PHASE 2 NEXT SPRINT**
- Coordinate with backend team for FCM integration
- Proper real-time notifications (< 1 second)
- Production-ready solution
- Better user experience

Would you like me to implement Phase 1 (polling) now?
