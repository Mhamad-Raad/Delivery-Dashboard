# Mall-Dash-Mobile Design System Implementation Guide

## Overview

This document outlines the centralized design system implemented for the Mall-Dash-Mobile Flutter application. The design system provides:

- **Centralized styling** - All colors, spacing, typography, and shadows defined in one place
- **Dark/Light mode support** - Seamless theme switching with proper color variants
- **Reusable components** - Shared widgets that eliminate code duplication
- **Consistent UX** - Uniform styling across all 21+ screens

## Architecture

```
lib/src/core/
├── design/                          # Design tokens
│   ├── app_colors.dart             # Color palette
│   ├── app_spacing.dart            # Spacing, gaps, padding
│   ├── app_radius.dart             # Border radius constants
│   ├── app_shadows.dart            # Elevation/shadow definitions
│   ├── app_typography.dart         # Text styles and sizes
│   └── design_system.dart          # Barrel export
├── theme/
│   └── custom_theme_extension.dart # Semantic colors for ThemeData
└── widgets/                         # Shared UI components
    ├── status_badge.dart           # Order/ticket status badges
    ├── app_card.dart               # Card variants
    ├── info_row.dart               # Information display rows
    ├── empty_state.dart            # Empty state placeholder
    ├── error_state.dart            # Error state with retry
    ├── loading_indicator.dart      # Loading spinners
    ├── section_header.dart         # Section headers
    └── widgets.dart                # Barrel export
```

## Color System

### Primary Color
- **Professional Blue**: `0xFF1565C0` (Material Blue 800)
- Primary container: `0xFFBBDEFB` (Blue 100)

### Semantic Colors (CustomThemeExtension)

```dart
// Access via context extension
final appTheme = context.appTheme;

// Available semantic colors:
appTheme.successColor      // Green - success states
appTheme.warningColor      // Orange - warnings, pending
appTheme.errorColor        // Red - errors, cancelled
appTheme.infoColor         // Blue - information
appTheme.textPrimary       // Main text color
appTheme.textSecondary     // Secondary text
appTheme.textTertiary      // Muted text
appTheme.dividerColor      // Dividers
appTheme.surfaceVariant    // Surface variants

// Order status colors
appTheme.orderPendingColor
appTheme.orderConfirmedColor
appTheme.orderPreparingColor
appTheme.orderOutForDeliveryColor
appTheme.orderDeliveredColor
appTheme.orderCancelledColor
appTheme.orderPickedUpColor
appTheme.orderInTransitColor

// Helper method for status codes
appTheme.getOrderStatusColor(statusCode)
```

### Using Colors

```dart
// ❌ DON'T - hardcoded colors
color: Colors.green
color: Colors.red
color: Colors.grey[600]

// ✅ DO - semantic colors
final appTheme = context.appTheme;
color: appTheme.successColor
color: appTheme.errorColor
color: appTheme.textSecondary

// ✅ DO - theme colors
color: Theme.of(context).colorScheme.primary
color: Theme.of(context).colorScheme.error
```

## Spacing System

### Spacing Values

```dart
AppSpacing.xxs  // 2.0
AppSpacing.xs   // 4.0
AppSpacing.sm   // 8.0
AppSpacing.md   // 16.0 (default)
AppSpacing.lg   // 24.0
AppSpacing.xl   // 32.0
AppSpacing.xxl  // 48.0
```

### Pre-built EdgeInsets

```dart
// All sides
padding: AppSpacing.allXs   // 4.0 all
padding: AppSpacing.allSm   // 8.0 all
padding: AppSpacing.allMd   // 16.0 all (most common)
padding: AppSpacing.allLg   // 24.0 all

// Horizontal only
padding: AppSpacing.horizontalMd  // horizontal 16.0

// Vertical only
padding: AppSpacing.verticalSm    // vertical 8.0
```

### Pre-built SizedBox Gaps

```dart
// Vertical gaps
AppSpacing.verticalGapXs   // SizedBox(height: 4)
AppSpacing.verticalGapSm   // SizedBox(height: 8)
AppSpacing.verticalGapMd   // SizedBox(height: 16)
AppSpacing.verticalGapLg   // SizedBox(height: 24)

// Horizontal gaps
AppSpacing.horizontalGapXs  // SizedBox(width: 4)
AppSpacing.horizontalGapSm  // SizedBox(width: 8)
AppSpacing.horizontalGapMd  // SizedBox(width: 16)
AppSpacing.horizontalGapLg  // SizedBox(width: 24)
```

### Icon Sizes

```dart
AppSpacing.iconSm     // 16.0
AppSpacing.iconMd     // 24.0
AppSpacing.iconLg     // 32.0
AppSpacing.iconAvatar // 80.0
```

## Border Radius

```dart
// Raw values
AppRadius.xs   // 4.0
AppRadius.sm   // 8.0
AppRadius.md   // 12.0
AppRadius.lg   // 16.0
AppRadius.xl   // 24.0
AppRadius.pill // 100.0 (fully rounded)

// Pre-built BorderRadius
AppRadius.radiusXs    // BorderRadius.circular(4)
AppRadius.radiusSm    // BorderRadius.circular(8)
AppRadius.radiusMd    // BorderRadius.circular(12)
AppRadius.radiusPill  // BorderRadius.circular(100)

// Shaped borders for specific components
AppRadius.cardShape         // RoundedRectangleBorder(12)
AppRadius.buttonShape       // RoundedRectangleBorder(8)
AppRadius.dialogShape       // RoundedRectangleBorder(16)
AppRadius.bottomSheetShape  // Top corners only (24)
```

## Shadows / Elevation

```dart
AppShadows.elevationNone  // 0.0
AppShadows.elevationXs    // 1.0
AppShadows.elevationSm    // 2.0
AppShadows.elevationMd    // 4.0
AppShadows.elevationLg    // 8.0
AppShadows.elevationXl    // 16.0
```

## Shared Widgets

### StatusBadge / StatusBadgeSolid

For displaying order/ticket status consistently:

```dart
// Outline style
StatusBadge(
  label: order.statusName,
  statusCode: order.status,
  size: StatusBadgeSize.small,
)

// Solid background style
StatusBadgeSolid(
  label: ticket.statusName,
  statusCode: ticket.status,
  size: StatusBadgeSize.medium,
)

// Custom color (not status-based)
StatusBadge.custom(
  label: 'DRIVER',
  color: appTheme.infoColor,
)
```

### EmptyState

For empty list states:

```dart
EmptyState(
  icon: Icons.shopping_cart_outlined,
  title: 'No orders yet',
  subtitle: 'Your orders will appear here',
  actionLabel: 'Browse Products',  // optional
  onAction: () => ...,             // optional
)
```

### ErrorState

For error states with retry:

```dart
ErrorState(
  title: 'Error loading data',
  message: error.toString(),
  onRetry: () => ref.refresh(provider),
)
```

### LoadingIndicator

For loading states:

```dart
// Full-page centered spinner
const LoadingIndicator()

// Small inline spinner
const LoadingIndicatorSmall()
```

### InfoRow / LabeledInfoRow

For displaying key-value information:

```dart
// Basic info row
InfoRow(
  icon: Icons.email,
  text: user.email,
)

// With label
LabeledInfoRow(
  label: 'Email',
  value: user.email,
  icon: Icons.email,
)
```

### SectionHeader

For section titles:

```dart
SectionHeader(
  title: 'Recent Orders',
  action: TextButton(
    onPressed: () => ...,
    child: Text('See All'),
  ),
)
```

## Theme Configuration

The app uses Material 3 with comprehensive component themes defined in `main.dart`:

- **AppBar**: Surface color background, on-surface text
- **Card**: Subtle elevation (2.0), 12px border radius
- **ElevatedButton**: Primary color, 8px radius
- **OutlinedButton**: Primary outline, 8px radius
- **TextField**: Outlined style with error states
- **Dialog/BottomSheet**: 16px/24px radius
- **Chip**: 100px pill shape

### Supporting Dark Mode

All screens should use semantic colors that automatically adapt:

```dart
// ✅ These adapt automatically
color: appTheme.textPrimary  // Dark on light, light on dark
color: Theme.of(context).colorScheme.surface

// ❌ These don't adapt
color: Colors.black          // Always black
color: Color(0xFF333333)     // Always dark grey
```

## Migration Checklist

When updating existing screens:

1. **Add imports**:
   ```dart
   import '../../../core/design/design_system.dart';
   import '../../../core/theme/custom_theme_extension.dart';
   ```

2. **Replace hardcoded padding**:
   ```dart
   // Before
   padding: EdgeInsets.all(16.0)
   
   // After
   padding: AppSpacing.allMd
   ```

3. **Replace hardcoded colors**:
   ```dart
   // Before
   color: Colors.green
   backgroundColor: Colors.red
   
   // After
   final appTheme = context.appTheme;
   color: appTheme.successColor
   backgroundColor: appTheme.errorColor
   ```

4. **Replace status color methods**:
   ```dart
   // Before
   Color _getStatusColor(int status) {
     switch(status) {
       case 1: return Colors.orange;
       ...
     }
   }
   
   // After - use StatusBadge widget or appTheme
   StatusBadgeSolid(
     label: statusName,
     statusCode: status,
   )
   // Or
   color: appTheme.getOrderStatusColor(status)
   ```

5. **Replace spacing SizedBoxes**:
   ```dart
   // Before
   SizedBox(height: 16)
   SizedBox(width: 8)
   
   // After
   AppSpacing.verticalGapMd
   AppSpacing.horizontalGapSm
   ```

6. **Replace BorderRadius**:
   ```dart
   // Before
   borderRadius: BorderRadius.circular(12)
   
   // After
   borderRadius: AppRadius.radiusMd
   ```

7. **Use shared widgets**:
   ```dart
   // Before - inline loading
   if (loading) return CircularProgressIndicator();
   
   // After
   if (loading) return const LoadingIndicator();
   
   // Before - inline empty state
   if (list.isEmpty) return Text('No items');
   
   // After
   if (list.isEmpty) return EmptyState(
     icon: Icons.list,
     title: 'No items',
   );
   ```

## Files Updated

### Core Design System (New)
- `lib/src/core/design/app_colors.dart`
- `lib/src/core/design/app_spacing.dart`
- `lib/src/core/design/app_radius.dart`
- `lib/src/core/design/app_shadows.dart`
- `lib/src/core/design/app_typography.dart`
- `lib/src/core/design/design_system.dart`

### Theme Extension (Updated)
- `lib/src/core/theme/custom_theme_extension.dart` - 19 semantic colors

### Shared Widgets (New)
- `lib/src/core/widgets/status_badge.dart`
- `lib/src/core/widgets/app_card.dart`
- `lib/src/core/widgets/info_row.dart`
- `lib/src/core/widgets/empty_state.dart`
- `lib/src/core/widgets/error_state.dart`
- `lib/src/core/widgets/loading_indicator.dart`
- `lib/src/core/widgets/section_header.dart`
- `lib/src/core/widgets/widgets.dart`

### Theme (Updated)
- `lib/main.dart` - Complete theme with Blue primary color

### Screens Updated
- `lib/src/features/auth/presentation/login_page.dart`
- `lib/src/features/home/presentation/home_page.dart`
- `lib/src/features/orders/presentation/orders_page.dart`
- `lib/src/features/profile/presentation/profile_page.dart`
- `lib/src/features/settings/presentation/settings_page.dart`
- `lib/src/core/presentation/main_scaffold.dart` (Material Icons only)
- `lib/src/features/driver/presentation/driver_home_page.dart`
- `lib/src/features/driver/presentation/active_deliveries_page.dart`
- `lib/src/features/driver/presentation/available_orders_page.dart`
- `lib/src/features/driver/presentation/delivery_details_page.dart`
- `lib/src/features/driver/presentation/driver_profile_page.dart`
- `lib/src/features/support/presentation/support_tickets_page.dart`
- `lib/src/features/support/presentation/support_ticket_details_page.dart`
- `lib/src/features/profile/presentation/view_profile_page.dart`
- `lib/src/features/profile/presentation/edit_profile_page.dart`
- `lib/src/features/order/presentation/order_details_page.dart`

## Icons

The app uses **Material Icons only**. Font Awesome has been removed from navigation.

```dart
// ✅ Use Material Icons
Icon(Icons.home_outlined)
Icon(Icons.notifications_outlined)
Icon(Icons.person_outlined)

// ❌ Don't use Font Awesome
// FaIcon(FontAwesomeIcons.house)
```

## Quick Reference

| Token | Value | Usage |
|-------|-------|-------|
| `AppSpacing.md` | 16.0 | Default padding |
| `AppSpacing.allMd` | EdgeInsets(16) | Card/page padding |
| `AppSpacing.verticalGapMd` | SizedBox(h:16) | Vertical spacing |
| `AppRadius.radiusMd` | 12.0 | Card corners |
| `AppRadius.radiusSm` | 8.0 | Button corners |
| `AppShadows.elevationSm` | 2.0 | Card elevation |
| `appTheme.successColor` | Green | Success states |
| `appTheme.errorColor` | Red | Error states |
| `appTheme.warningColor` | Orange | Warning states |
| `appTheme.textSecondary` | Grey | Muted text |

---

**Last Updated**: Design system implementation complete with Professional Blue theme, dark/light mode support, and 16+ screens migrated.
