# Logout & Navigation Stack Fix

## Problem

When logging out from driver account:
1. ✅ Logout works - user sees login page
2. ❌ Pressing back button returns to driver pages
3. ❌ Old driver pages still try to fetch data (start shifts, get orders)
4. ❌ Creates confusion and potential data leaks

**Logs showed:**
```
Starting driver shift...
Start shift response status: 200
Getting queue position...
Ending driver shift...
```

This means the old DriverHomePage was still active in the navigation stack!

---

## Root Cause

### Before Fix:

```
Navigation Stack:
[LoginPage] ← app starts here
    ↓ (user logs in)
[MainScaffold → DriverHomePage] ← driver sees this
    ↓ (user navigates to profile)
[MainScaffold → DriverHomePage → DriverProfilePage]
    ↓ (user clicks logout)
[LoginPage] ← back button still has access to old stack!

When pressing back:
[LoginPage] → [DriverProfilePage] → [DriverHomePage] 
            ↑ OLD PAGES STILL EXIST!
```

The problem is that `AuthWidget` switches between `LoginPage` and `MainScaffold`, but **doesn't clear the old widget tree**. The previous pages remain in memory and in the navigation stack.

---

## Solution

### After Fix:

Use **unique keys** based on auth status to force complete widget tree rebuilds:

```dart
@override
Widget build(BuildContext context) {
  final authStatus = ref.watch(authNotifierProvider);
  
  // ✅ Key changes when auth status changes
  final widgetKey = ValueKey('auth_${authStatus.name}');
  
  switch (authStatus) {
    case AuthStatus.initial:
      return Scaffold(key: widgetKey, ...);
    case AuthStatus.authenticated:
      return MainScaffold(key: widgetKey); // ✅ New key = new tree
    case AuthStatus.unauthenticated:
      return LoginPage(key: widgetKey);    // ✅ New key = new tree
  }
}
```

### How It Works:

1. **Login:** `authStatus` changes from `unauthenticated` → `authenticated`
   - Key changes from `'auth_unauthenticated'` → `'auth_authenticated'`
   - Flutter sees different key, disposes old LoginPage
   - Creates fresh MainScaffold with empty navigation stack

2. **Logout:** `authStatus` changes from `authenticated` → `unauthenticated`
   - Key changes from `'auth_authenticated'` → `'auth_unauthenticated'`
   - Flutter sees different key, disposes entire MainScaffold tree
   - Creates fresh LoginPage with empty navigation stack

3. **No Back Navigation:**
   - Old pages completely disposed
   - Navigation stack cleared
   - Back button has nowhere to go (will exit app)

---

## Testing Checklist

### ✅ Test Logout Flow

1. **Login as Driver**
   - [ ] App shows DriverHomePage
   - [ ] Start a shift
   - [ ] Navigate to DriverProfilePage

2. **Logout**
   - [ ] Click "Logout" button
   - [ ] Confirm logout dialog
   - [ ] App shows LoginPage

3. **Test Back Button**
   - [ ] Press device/system back button
   - [ ] Should exit app (or show system dialog)
   - [ ] Should NOT return to DriverHomePage
   - [ ] Check logs: NO shift starting messages

4. **Login Again**
   - [ ] Login with same or different account
   - [ ] Fresh driver dashboard loads
   - [ ] No old shift data

### ✅ Test Login Flow

1. **Start at Login**
   - [ ] Enter credentials
   - [ ] Click login button
   - [ ] App shows driver/tenant dashboard

2. **Test Back Button**
   - [ ] Press back button
   - [ ] Should exit app
   - [ ] Should NOT return to LoginPage

---

## Technical Details

### File Modified:
- `lib/src/features/auth/presentation/auth_widget.dart`

### Key Concepts:

#### Widget Keys in Flutter
```dart
// Without key - Flutter reuses existing widget
return MyWidget();

// With same key - Flutter reuses existing widget
return MyWidget(key: ValueKey('same'));

// With different key - Flutter disposes old, creates new
return MyWidget(key: ValueKey('different'));
```

#### ValueKey vs ObjectKey vs GlobalKey
- **ValueKey:** Uses a value (string, int, etc.) for equality
  - Perfect for our use case: different auth states = different values
- **ObjectKey:** Uses object identity
- **GlobalKey:** Maintains state across tree changes
  - NOT what we want (we want to clear state)

#### Why This Works for Navigation
When Flutter sees a different key:
1. Calls `dispose()` on old widget
2. Calls `deactivate()` on all children
3. Removes all descendant widgets from tree
4. Clears any navigation routes from old tree
5. Creates completely fresh widget tree

---

## Alternative Solutions (Not Used)

### ❌ Navigator.pushAndRemoveUntil()
```dart
// Would require changing every logout call
Navigator.pushAndRemoveUntil(
  context,
  MaterialPageRoute(builder: (context) => LoginPage()),
  (route) => false,
);
```
**Problem:** Doesn't work with AuthWidget pattern where root widget changes

### ❌ SystemNavigator.pop()
```dart
// Would exit the app instead of going to login
await SystemNavigator.pop();
```
**Problem:** User has to restart app

### ❌ Custom Navigator Key
```dart
final navigatorKey = GlobalKey<NavigatorState>();
navigatorKey.currentState?.pushAndRemoveUntil(...);
```
**Problem:** Complex, requires passing key through widget tree

### ✅ Widget Keys (Used)
**Advantages:**
- Simple, declarative
- Leverages Flutter's natural widget lifecycle
- No manual navigation management
- Works perfectly with AuthWidget pattern
- Automatically clears all state

---

## Related Fixes

### Logout Context Issue (Fixed Previously)
The "End Shift & Logout" button was also fixed to use proper context handling:
- Returns action string from dialog
- Executes logout with valid page context
- Shows loading indicator during shift end

### Token Management (Fixed Previously)
- Tokens validated on startup
- Automatic token refresh on 401
- Logout triggered when tokens expire
- Backend logout endpoint called

---

## Common Issues & Solutions

### Issue: "Back button still shows old pages"
**Cause:** Key not changing properly  
**Solution:** Check that `authStatus` actually changes in AuthNotifier

### Issue: "App crashes on logout"
**Cause:** Trying to use disposed context  
**Solution:** Ensure all async operations check `context.mounted`

### Issue: "Login page flickers"
**Cause:** Auth status changing multiple times rapidly  
**Solution:** Check AuthNotifier doesn't call `checkAuthStatus` multiple times

### Issue: "Old data still visible after login"
**Cause:** Riverpod providers not resetting  
**Solution:** Keys ensure fresh provider scope (working as intended)

---

## Debug Logging

To verify the fix works, check logs:

**Expected after logout:**
```
[AuthNotifier] Logout initiated
[AuthRepository] Calling backend logout endpoint
[AuthRepository] Clearing local tokens
[AuthNotifier] Auth state set to unauthenticated
```

**NOT expected after logout + back button:**
```
Starting driver shift...        ← Should NOT appear
Getting queue position...       ← Should NOT appear
Getting orders...               ← Should NOT appear
```

If you see shift/order messages after logout, the old pages are still active!

---

## Summary

### Before Fix:
- ❌ Logout left old pages in navigation stack
- ❌ Back button accessed disposed pages
- ❌ Pages tried to fetch data with invalid tokens
- ❌ Confusion about logged-in state

### After Fix:
- ✅ Logout completely disposes old widget tree
- ✅ Back button has nowhere to go (clean state)
- ✅ No unauthorized API calls
- ✅ Clear separation between sessions

### How to Test:
1. Login → Logout → Press back button
2. Should exit app or show no previous pages
3. Check logs: No shift/order API calls after logout

---

**Implementation Date:** January 17, 2026  
**Files Modified:** 1 file (auth_widget.dart)  
**Impact:** Critical security and UX improvement  
**Status:** ✅ Ready for testing
