import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';

//  Notification Model (local, ready for provider integration) 

enum NotificationType { order, promo, system }

class AppNotification {
  final String id;
  final String title;
  final String body;
  final NotificationType type;
  final DateTime timestamp;
  final bool isRead;

  const AppNotification({
    required this.id,
    required this.title,
    required this.body,
    required this.type,
    required this.timestamp,
    this.isRead = false,
  });

  AppNotification copyWith({bool? isRead}) =>
      AppNotification(
        id: id,
        title: title,
        body: body,
        type: type,
        timestamp: timestamp,
        isRead: isRead ?? this.isRead,
      );
}

//  Page 

class NotificationsPage extends StatefulWidget {
  const NotificationsPage({super.key});

  @override
  State<NotificationsPage> createState() => _NotificationsPageState();
}

class _NotificationsPageState extends State<NotificationsPage> {
  // Local notification list  swap with a provider when backend is ready.
  late List<AppNotification> _notifications;

  @override
  void initState() {
    super.initState();
    _notifications = [];
  }

  //  Group by date 
  Map<String, List<AppNotification>> get _grouped {
    final map = <String, List<AppNotification>>{};
    for (final n in _notifications) {
      final key = _dateLabel(n.timestamp);
      map.putIfAbsent(key, () => []).add(n);
    }
    return map;
  }

  String _dateLabel(DateTime date) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final d = DateTime(date.year, date.month, date.day);
    if (d == today) return 'Today';
    if (d == today.subtract(const Duration(days: 1))) return 'Yesterday';
    return '${date.day}/${date.month}/${date.year}';
  }

  void _dismiss(String id) {
    setState(() {
      _notifications.removeWhere((n) => n.id == id);
    });
  }

  void _markAsRead(String id) {
    setState(() {
      _notifications = _notifications
          .map((n) => n.id == id ? n.copyWith(isRead: true) : n)
          .toList();
    });
  }

  //  Type helpers 
  IconData _typeIcon(NotificationType type) {
    switch (type) {
      case NotificationType.order:
        return Icons.receipt_long_rounded;
      case NotificationType.promo:
        return Icons.local_offer_rounded;
      case NotificationType.system:
        return Icons.info_rounded;
    }
  }

  Color _typeColor(NotificationType type) {
    switch (type) {
      case NotificationType.order:
        return AppColors.info;
      case NotificationType.promo:
        return AppColors.accent;
      case NotificationType.system:
        return AppColors.orderPreparing;
    }
  }

  //  Build 
  @override
  Widget build(BuildContext context) {
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0,
        leading: Padding(
          padding: const EdgeInsets.only(left: 12),
          child: Center(
            child: GestureDetector(
              onTap: () => Navigator.pop(context),
              child: Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: (isDark ? Colors.white : Colors.black)
                      .withAlpha(isDark ? 25 : 15),
                  borderRadius: AppRadius.radiusPill,
                  border: Border.all(
                    color: (isDark ? Colors.white : Colors.black).withAlpha(10),
                  ),
                ),
                child: ClipRRect(
                  borderRadius: AppRadius.radiusPill,
                  child: BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 10, sigmaY: 10),
                    child: Icon(Icons.arrow_back_rounded,
                        size: 20,
                        color: isDark ? Colors.white : Colors.black87),
                  ),
                ),
              ),
            ),
          ),
        ),
        title: Text(
          'Notifications',
          style: GoogleFonts.inter(
            fontSize: 17,
            fontWeight: FontWeight.w700,
            letterSpacing: -0.3,
          ),
        ),
        centerTitle: true,
        actions: [
          if (_notifications.any((n) => !n.isRead))
            Padding(
              padding: const EdgeInsets.only(right: 12),
              child: Center(
                child: GestureDetector(
                  onTap: () {
                    setState(() {
                      _notifications = _notifications
                          .map((n) => n.copyWith(isRead: true))
                          .toList();
                    });
                  },
                  child: Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: AppColors.primary.withAlpha(isDark ? 60 : 15),
                      borderRadius: AppRadius.radiusPill,
                    ),
                    child: Text(
                      'Read all',
                      style: GoogleFonts.inter(
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                        color: isDark ? AppColors.accentLight : AppColors.primary,
                      ),
                    ),
                  ),
                ),
              ),
            ),
        ],
      ),
      body: _notifications.isEmpty
          ? _buildEmptyState(isDark, appTheme)
          : _buildList(isDark, appTheme),
    );
  }

  //  Empty state 

  Widget _buildEmptyState(bool isDark, CustomThemeExtension appTheme) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 100,
            height: 100,
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: isDark
                    ? [AppColors.surfaceVariantDark, AppColors.surfaceDark]
                    : [AppColors.surfaceVariant, AppColors.surface],
              ),
              borderRadius: AppRadius.radiusCircle,
            ),
            child: Icon(Icons.notifications_off_outlined,
                size: 44, color: appTheme.textTertiary),
          )
              .animate()
              .scale(
                begin: const Offset(0.8, 0.8),
                end: const Offset(1, 1),
                duration: 600.ms,
                curve: Curves.elasticOut,
              )
              .fadeIn(duration: 400.ms),
          AppSpacing.verticalGapLg,
          Text(
            'All caught up!',
            style: GoogleFonts.inter(
              fontSize: 20,
              fontWeight: FontWeight.w800,
              letterSpacing: -0.3,
              color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
            ),
          ).animate().fadeIn(delay: 200.ms, duration: 400.ms),
          AppSpacing.verticalGapXs,
          Text(
            'You have no notifications right now',
            style: GoogleFonts.inter(
              fontSize: 14,
              color: appTheme.textSecondary,
            ),
          ).animate().fadeIn(delay: 300.ms, duration: 400.ms),
        ],
      ),
    );
  }

  //  Notification list 

  Widget _buildList(bool isDark, CustomThemeExtension appTheme) {
    final groups = _grouped;
    final keys = groups.keys.toList();

    return ListView.builder(
      physics: const BouncingScrollPhysics(),
      padding: EdgeInsets.only(
        top: MediaQuery.of(context).padding.top + kToolbarHeight + 12,
        bottom: 40,
      ),
      itemCount: keys.length,
      itemBuilder: (context, sectionIndex) {
        final dateKey = keys[sectionIndex];
        final items = groups[dateKey]!;

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Section header
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
              child: Text(
                dateKey,
                style: GoogleFonts.inter(
                  fontSize: 13,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.5,
                  color: appTheme.textTertiary,
                ),
              ),
            ),
            ...items.asMap().entries.map(
              (entry) {
                final idx = entry.key;
                final n = entry.value;
                return _buildNotificationTile(n, isDark, appTheme)
                    .animate()
                    .fadeIn(
                      duration: 400.ms,
                      delay: Duration(milliseconds: 80 * idx),
                    )
                    .slideX(begin: 0.05, end: 0);
              },
            ),
          ],
        );
      },
    );
  }

  Widget _buildNotificationTile(
    AppNotification n,
    bool isDark,
    CustomThemeExtension appTheme,
  ) {
    final color = _typeColor(n.type);

    return Dismissible(
      key: ValueKey(n.id),
      direction: DismissDirection.endToStart,
      onDismissed: (_) => _dismiss(n.id),
      background: Container(
        margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
        decoration: BoxDecoration(
          color: AppColors.error.withAlpha(20),
          borderRadius: AppRadius.radiusXl,
        ),
        alignment: Alignment.centerRight,
        padding: const EdgeInsets.only(right: 24),
        child: Icon(Icons.delete_outline_rounded,
            color: AppColors.error, size: 24),
      ),
      child: GestureDetector(
        onTap: () => _markAsRead(n.id),
        child: Container(
          margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            color: isDark ? AppColors.cardBackgroundDark : AppColors.cardBackground,
            borderRadius: AppRadius.radiusXl,
            boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
            border: Border.all(
              color: !n.isRead
                  ? color.withAlpha(40)
                  : (isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5)),
            ),
          ),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Type icon
              Container(
                width: 42,
                height: 42,
                decoration: BoxDecoration(
                  color: color.withAlpha(25),
                  borderRadius: AppRadius.radiusMd,
                ),
                child: Icon(_typeIcon(n.type), size: 20, color: color),
              ),
              const SizedBox(width: 12),
              // Content
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            n.title,
                            style: GoogleFonts.inter(
                              fontSize: 14,
                              fontWeight:
                                  n.isRead ? FontWeight.w500 : FontWeight.w700,
                              color: isDark
                                  ? AppColors.textPrimaryDark
                                  : AppColors.textPrimary,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        // Unread dot
                        if (!n.isRead) ...[
                          const SizedBox(width: 8),
                          Container(
                            width: 8,
                            height: 8,
                            decoration: BoxDecoration(
                              color: color,
                              shape: BoxShape.circle,
                            ),
                          ),
                        ],
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      n.body,
                      style: GoogleFonts.inter(
                        fontSize: 13,
                        color: appTheme.textSecondary,
                        height: 1.35,
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 6),
                    Text(
                      _formatTime(n.timestamp),
                      style: GoogleFonts.inter(
                        fontSize: 11,
                        color: appTheme.textTertiary,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  String _formatTime(DateTime dt) {
    final h = dt.hour.toString().padLeft(2, '0');
    final m = dt.minute.toString().padLeft(2, '0');
    return '$h:$m';
  }
}
